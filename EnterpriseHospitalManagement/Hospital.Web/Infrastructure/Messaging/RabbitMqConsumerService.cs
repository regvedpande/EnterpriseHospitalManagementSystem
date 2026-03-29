using Hospital.Web.Infrastructure.Messaging.Messages;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace Hospital.Web.Infrastructure.Messaging;

/// <summary>
/// Hosted service that consumes messages from RabbitMQ and dispatches
/// notification emails for each domain event.
/// Falls back gracefully when RabbitMQ is not configured.
/// </summary>
public sealed class RabbitMqConsumerService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly IConfiguration   _config;
    private readonly ILogger<RabbitMqConsumerService> _log;
    private IConnection? _connection;
    private IModel?      _channel;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public RabbitMqConsumerService(
        IServiceProvider services,
        IConfiguration   config,
        ILogger<RabbitMqConsumerService> log)
    {
        _services = services;
        _config   = config;
        _log      = log;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var connStr = _config["RabbitMQ:ConnectionString"];
        if (string.IsNullOrWhiteSpace(connStr))
        {
            _log.LogInformation("[RabbitMQ Consumer] Not configured — notification consumer idle");
            return;
        }

        // Retry connecting up to 5 times with back-off
        for (int attempt = 1; attempt <= 5 && !stoppingToken.IsCancellationRequested; attempt++)
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    Uri = new Uri(connStr),
                    DispatchConsumersAsync   = true,
                    AutomaticRecoveryEnabled = true
                };
                _connection = factory.CreateConnection("MedCoreHMS-Consumer");
                _channel    = _connection.CreateModel();

                // Ensure exchange + queues exist (idempotent)
                _channel.ExchangeDeclare(RabbitMqMessageBus.ExchangeName, ExchangeType.Topic, durable: true);
                foreach (var (queue, key) in new[] {
                    ("hms.appointments", "appointment.*"),
                    ("hms.billing",      "bill.*"),
                    ("hms.labs",         "lab.*"),
                    ("hms.users",        "user.*")
                })
                {
                    _channel.QueueDeclare(queue, durable: true, exclusive: false, autoDelete: false);
                    _channel.QueueBind(queue, RabbitMqMessageBus.ExchangeName, key);
                }

                // Prefetch 1 — process one message at a time per consumer instance
                _channel.BasicQos(0, 1, false);

                var consumer = new AsyncEventingBasicConsumer(_channel);
                consumer.Received += OnMessageReceivedAsync;

                foreach (var queue in new[] { "hms.appointments", "hms.billing", "hms.labs", "hms.users" })
                    _channel.BasicConsume(queue, autoAck: false, consumer: consumer);

                _log.LogInformation("[RabbitMQ Consumer] Listening on queues: appointments, billing, labs, users");
                break;
            }
            catch (Exception ex)
            {
                _log.LogWarning(ex, "[RabbitMQ Consumer] Connect attempt {A}/5 failed", attempt);
                await Task.Delay(TimeSpan.FromSeconds(attempt * 3), stoppingToken);
            }
        }

        // Keep the service alive until cancellation
        await Task.Delay(Timeout.Infinite, stoppingToken).ConfigureAwait(false);
    }

    private async Task OnMessageReceivedAsync(object sender, BasicDeliverEventArgs ea)
    {
        var routingKey = ea.RoutingKey;
        try
        {
            var body = Encoding.UTF8.GetString(ea.Body.Span);
            _log.LogDebug("[RabbitMQ Consumer] Received '{RoutingKey}'", routingKey);

            await using var scope   = _services.CreateAsyncScope();
            var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();

            // Dispatch based on routing key
            await (routingKey switch
            {
                "appointment.created"        => HandleAppointmentCreated(body, emailSender),
                "appointment.status_changed" => HandleAppointmentStatusChanged(body, emailSender),
                "bill.generated"             => HandleBillGenerated(body, emailSender),
                "lab.result_ready"           => HandleLabResultReady(body, emailSender),
                "user.registered"            => HandleUserRegistered(body, emailSender),
                _                            => Task.CompletedTask
            });

            _channel?.BasicAck(ea.DeliveryTag, multiple: false);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "[RabbitMQ Consumer] Error handling '{RoutingKey}'", routingKey);
            _channel?.BasicNack(ea.DeliveryTag, multiple: false, requeue: false); // dead-letter
        }
    }

    private async Task HandleAppointmentCreated(string body, IEmailSender email)
    {
        var msg = JsonSerializer.Deserialize<AppointmentCreatedMessage>(body, JsonOpts);
        if (msg == null || string.IsNullOrWhiteSpace(msg.PatientEmail)) return;
        await email.SendEmailAsync(msg.PatientEmail, "Appointment Confirmation — MedCore HMS",
            $"<h2>Hi {msg.PatientName},</h2>" +
            $"<p>Your appointment with <strong>Dr. {msg.DoctorName}</strong> has been scheduled.</p>" +
            $"<ul><li><strong>Date:</strong> {msg.AppointmentDate:dddd, MMMM d, yyyy h:mm tt}</li>" +
            $"<li><strong>Type:</strong> {msg.AppointmentType}</li>" +
            $"<li><strong>Appointment #:</strong> {msg.AppointmentId}</li></ul>" +
            $"<p>Please arrive 10 minutes early. Contact us if you need to reschedule.</p>" +
            $"<p>— MedCore HMS Team</p>");
    }

    private async Task HandleAppointmentStatusChanged(string body, IEmailSender email)
    {
        var msg = JsonSerializer.Deserialize<AppointmentStatusChangedMessage>(body, JsonOpts);
        if (msg == null || string.IsNullOrWhiteSpace(msg.PatientEmail)) return;
        await email.SendEmailAsync(msg.PatientEmail, $"Appointment {msg.NewStatus} — MedCore HMS",
            $"<h2>Hi {msg.PatientName},</h2>" +
            $"<p>Your appointment with <strong>Dr. {msg.DoctorName}</strong> status has changed " +
            $"from <em>{msg.OldStatus}</em> to <strong>{msg.NewStatus}</strong>.</p>" +
            $"<p>Log in to your patient portal for details.</p>" +
            $"<p>— MedCore HMS Team</p>");
    }

    private async Task HandleBillGenerated(string body, IEmailSender email)
    {
        var msg = JsonSerializer.Deserialize<BillGeneratedMessage>(body, JsonOpts);
        if (msg == null || string.IsNullOrWhiteSpace(msg.PatientEmail)) return;
        await email.SendEmailAsync(msg.PatientEmail, "New Bill Generated — MedCore HMS",
            $"<h2>Hi {msg.PatientName},</h2>" +
            $"<p>A new bill has been generated for your account.</p>" +
            $"<ul><li><strong>Bill #:</strong> {msg.BillNumber}</li>" +
            $"<li><strong>Amount:</strong> ${msg.TotalAmount:N2}</li></ul>" +
            $"<p>Please log in to your patient portal to review and pay your bill.</p>" +
            $"<p>— MedCore HMS Billing Team</p>");
    }

    private async Task HandleLabResultReady(string body, IEmailSender email)
    {
        var msg = JsonSerializer.Deserialize<LabResultReadyMessage>(body, JsonOpts);
        if (msg == null || string.IsNullOrWhiteSpace(msg.PatientEmail)) return;
        await email.SendEmailAsync(msg.PatientEmail, "Lab Results Ready — MedCore HMS",
            $"<h2>Hi {msg.PatientName},</h2>" +
            $"<p>Your lab results for <strong>{msg.TestName}</strong> are now available.</p>" +
            $"<p>Please log in to your patient portal to view the full report.</p>" +
            $"<p>— MedCore HMS Lab Team</p>");
    }

    private async Task HandleUserRegistered(string body, IEmailSender email)
    {
        var msg = JsonSerializer.Deserialize<UserRegisteredMessage>(body, JsonOpts);
        if (msg == null || string.IsNullOrWhiteSpace(msg.Email)) return;
        await email.SendEmailAsync(msg.Email, "Welcome to MedCore HMS",
            $"<h2>Welcome, {msg.Name}!</h2>" +
            $"<p>Your patient account has been created. You can now:</p>" +
            $"<ul><li>Book appointments with doctors</li><li>View lab results and reports</li>" +
            $"<li>Manage your bills and insurance</li></ul>" +
            $"<p>— MedCore HMS Team</p>");
    }

    public override void Dispose()
    {
        try { _channel?.Close(); _channel?.Dispose(); } catch { }
        try { _connection?.Close(); _connection?.Dispose(); } catch { }
        base.Dispose();
    }
}

using Hospital.Web.Infrastructure.Messaging.Messages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace Hospital.Web.Infrastructure.Messaging;

/// <summary>
/// RabbitMQ implementation of IMessageBus using topic exchange "hms.events".
/// Falls back gracefully (logs + returns) when RabbitMQ is not configured or unavailable.
/// </summary>
public sealed class RabbitMqMessageBus : IMessageBus, IDisposable
{
    public const string ExchangeName = "hms.events";

    private readonly ILogger<RabbitMqMessageBus> _log;
    private readonly string? _connectionString;
    private IConnection? _connection;
    private IModel? _channel;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private bool _disposed;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy        = JsonNamingPolicy.CamelCase,
        WriteIndented               = false
    };

    public RabbitMqMessageBus(IConfiguration config, ILogger<RabbitMqMessageBus> log)
    {
        _log = log;
        _connectionString = config["RabbitMQ:ConnectionString"];
        if (!string.IsNullOrWhiteSpace(_connectionString))
            TryConnect();
    }

    public bool IsConnected => _connection?.IsOpen == true && _channel?.IsOpen == true;

    public async Task PublishAsync<T>(string routingKey, T message, CancellationToken ct = default)
        where T : HmsMessage
    {
        if (string.IsNullOrWhiteSpace(_connectionString))
        {
            _log.LogDebug("[RabbitMQ] Not configured — skipping publish of '{RoutingKey}'", routingKey);
            return;
        }

        await _lock.WaitAsync(ct);
        try
        {
            EnsureChannel();
            if (_channel == null || !_channel.IsOpen)
            {
                _log.LogWarning("[RabbitMQ] Channel unavailable — skipping publish of '{RoutingKey}'", routingKey);
                return;
            }

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message, JsonOpts));

            var props = _channel.CreateBasicProperties();
            props.Persistent    = true;
            props.ContentType   = "application/json";
            props.MessageId     = message.MessageId.ToString();
            props.Type          = message.MessageType;
            props.Timestamp     = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

            _channel.BasicPublish(
                exchange:   ExchangeName,
                routingKey: routingKey,
                mandatory:  false,
                basicProperties: props,
                body:       body);

            _log.LogDebug("[RabbitMQ] Published '{RoutingKey}' MessageId={Id}", routingKey, message.MessageId);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "[RabbitMQ] Publish failed for '{RoutingKey}'", routingKey);
        }
        finally
        {
            _lock.Release();
        }
    }

    private void TryConnect()
    {
        try
        {
            var factory = new ConnectionFactory
            {
                Uri = new Uri(_connectionString!),
                DispatchConsumersAsync          = true,
                AutomaticRecoveryEnabled        = true,
                NetworkRecoveryInterval         = TimeSpan.FromSeconds(10)
            };
            _connection = factory.CreateConnection("MedCoreHMS-Publisher");
            _log.LogInformation("[RabbitMQ] Connected to broker");
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex, "[RabbitMQ] Could not connect to broker — running without message bus");
        }
    }

    private void EnsureChannel()
    {
        if (_connection == null || !_connection.IsOpen)
        {
            TryConnect();
            if (_connection == null) return;
        }

        if (_channel == null || !_channel.IsOpen)
        {
            _channel = _connection.CreateModel();
            // Declare durable topic exchange — idempotent, safe to call repeatedly
            _channel.ExchangeDeclare(ExchangeName, ExchangeType.Topic, durable: true, autoDelete: false);

            // Declare standard queues and bind them
            DeclareQueue("hms.appointments", "appointment.*");
            DeclareQueue("hms.billing",      "bill.*");
            DeclareQueue("hms.labs",         "lab.*");
            DeclareQueue("hms.users",        "user.*");
        }
    }

    private void DeclareQueue(string queueName, string bindingKey)
    {
        if (_channel == null) return;
        _channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false);
        _channel.QueueBind(queueName, ExchangeName, bindingKey);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        try { _channel?.Close(); _channel?.Dispose(); } catch { }
        try { _connection?.Close(); _connection?.Dispose(); } catch { }
        _lock.Dispose();
    }
}

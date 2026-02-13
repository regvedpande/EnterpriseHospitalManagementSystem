// Hospital.Utilities/EmailSender.cs
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System.Threading.Tasks;

namespace Hospital.Utilities
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _config;

        public EmailSender(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var smtp = _config.GetSection("Smtp");
            var host = smtp.GetValue<string>("Host");
            var port = smtp.GetValue<int?>("Port") ?? 25;
            var username = smtp.GetValue<string>("Username");
            var password = smtp.GetValue<string>("Password");
            var from = smtp.GetValue<string>("From") ?? username ?? "no-reply@example.com";
            var useSsl = smtp.GetValue<bool?>("UseSsl") ?? true;

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Enterprise Hospital", from));
            message.To.Add(MailboxAddress.Parse(email));
            message.Subject = subject;
            var body = new TextPart("html") { Text = htmlMessage ?? string.Empty };
            message.Body = body;

            using var client = new SmtpClient();
            // If host null/empty, throw a helpful exception (so DI consumer sees clear issue)
            if (string.IsNullOrWhiteSpace(host))
            {
                throw new InvalidOperationException("Smtp:Host is not configured. Please set Smtp configuration in appsettings.json or environment.");
            }

            await client.ConnectAsync(host, port, useSsl);

            if (!string.IsNullOrEmpty(username))
            {
                await client.AuthenticateAsync(username, password);
            }

            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}

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
            var port = smtp.GetValue<int>("Port");
            var username = smtp.GetValue<string>("Username");
            var password = smtp.GetValue<string>("Password");
            var from = smtp.GetValue<string>("From");
            var useSsl = smtp.GetValue<bool>("UseSsl");

            // defensive defaults
            var fromAddress = string.IsNullOrWhiteSpace(from) ? "no-reply@example.com" : from;
            var toAddress = string.IsNullOrWhiteSpace(email) ? "no-reply@example.com" : email;
            var subjectSafe = subject ?? string.Empty;
            var bodyHtml = htmlMessage ?? string.Empty;

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Enterprise Hospital", fromAddress));
            message.To.Add(MailboxAddress.Parse(toAddress));
            message.Subject = subjectSafe;
            message.Body = new TextPart("html") { Text = bodyHtml };

            using var client = new SmtpClient();
            // connect (MailKit supports the bool overload in current releases)
            await client.ConnectAsync(host, port, useSsl);

            if (!string.IsNullOrWhiteSpace(username))
            {
                await client.AuthenticateAsync(username, password);
            }

            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}

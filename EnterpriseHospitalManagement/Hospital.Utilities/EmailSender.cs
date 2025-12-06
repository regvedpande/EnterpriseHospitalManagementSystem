using Microsoft.AspNetCore.Identity.UI.Services;
using System.Threading.Tasks;

namespace Hospital.Utilities
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // TODO: Plug in SMTP / SendGrid / other email provider here
            return Task.CompletedTask;
        }
    }
}

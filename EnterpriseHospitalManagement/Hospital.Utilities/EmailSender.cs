using Microsoft.AspNetCore.Identity.UI.Services;
using System.Threading.Tasks;

namespace Hospital.Utilities
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // Implement email sending logic here
            return Task.CompletedTask;
        }
    }
}

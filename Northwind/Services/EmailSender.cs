using Microsoft.AspNetCore.Identity.UI.Services;

namespace Northwind.Services
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // Use an email service provider like SendGrid in production
            Console.WriteLine($"Sending email to {email}: {subject}");
            return Task.CompletedTask;
        }
    }
}

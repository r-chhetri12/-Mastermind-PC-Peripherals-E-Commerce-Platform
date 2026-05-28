using MailKit.Security;
using Microsoft.AspNetCore.Identity.UI.Services;
using MimeKit;
using MailKit.Net.Smtp;

namespace MasterMind.StaticData
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;

        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var emailToSend = new MimeMessage();
            emailToSend.From.Add(MailboxAddress.Parse(_configuration["EmailSettings:FromEmail"]));
            emailToSend.To.Add(MailboxAddress.Parse(email));
            emailToSend.Subject = subject;
            emailToSend.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = htmlMessage };

            using var emailClient = new SmtpClient();
            emailClient.Connect("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
            emailClient.Authenticate(
                _configuration["EmailSettings:FromEmail"],
                _configuration["EmailSettings:AppPassword"]
            );
            emailClient.Send(emailToSend);
            emailClient.Disconnect(true);

            return Task.CompletedTask;
        }
    }
}
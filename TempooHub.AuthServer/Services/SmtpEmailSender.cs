using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace TempooHub.AuthServer.Services
{

    public class SmtpEmailSender(IOptions<SmtpOptions> options) : IEmailSender
    {
        private readonly SmtpOptions _options = options.Value;

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            using var client = new SmtpClient(_options.Host, _options.Port)
            {
                EnableSsl = _options.EnableSsl,
            };

            if (!string.IsNullOrWhiteSpace(_options.User))
            {
                client.Credentials = new NetworkCredential(_options.User, _options.Password);
            }

            var mail = new MailMessage()
            {
                From = new MailAddress(_options.From),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true
            };
            mail.To.Add(email);

            await client.SendMailAsync(mail);
        }
    }
}

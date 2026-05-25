using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using PaceLetics.Web.Configuration;
using System.Net;
using System.Net.Mail;

namespace PaceLetics.Web.Services
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly SmtpOptions _options;

        public SmtpEmailSender(IOptions<SmtpOptions> options)
        {
            _options = options.Value;
            _options.Validate();
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var smtpPass = Environment.GetEnvironmentVariable(SmtpOptions.PasswordEnvironmentVariable);
            if (string.IsNullOrWhiteSpace(smtpPass))
                smtpPass = _options.Password;

            using var smtp = new SmtpClient(_options.Host)
            {
                Port = _options.Port,
                Credentials = new NetworkCredential(_options.User, smtpPass),
                EnableSsl = true
            };

            var mail = new MailMessage
            {
                From = new MailAddress(_options.Sender),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true
            };

            mail.To.Add(email);
            await smtp.SendMailAsync(mail);
        }
    }
}

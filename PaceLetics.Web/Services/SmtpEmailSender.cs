using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net;
using System.Net.Mail;

namespace PaceLetics.Web.Services
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly IConfiguration _config;

        public SmtpEmailSender(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var smtpHost = _config["Smtp:Host"];
            var smtpPort = int.Parse(GetRequiredSetting("Smtp:Port"));
            var smtpUser = _config["Smtp:User"];
            var smtpPass = Environment.GetEnvironmentVariable("PaceLeticsSmtpPw");
            var sender = _config["Smtp:Sender"];

            using var smtp = new SmtpClient(smtpHost ?? throw new InvalidOperationException("Missing SMTP host configuration."))
            {
                Port = smtpPort,
                Credentials = new NetworkCredential(smtpUser, smtpPass),
                EnableSsl = true
            };

            var mail = new MailMessage
            {
                From = new MailAddress(sender ?? throw new InvalidOperationException("Missing SMTP sender configuration.")),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true
            };

            mail.To.Add(email);
            await smtp.SendMailAsync(mail);
        }

        private string GetRequiredSetting(string key)
        {
            return _config[key] ?? throw new InvalidOperationException($"Missing configuration value '{key}'.");
        }
    }
}

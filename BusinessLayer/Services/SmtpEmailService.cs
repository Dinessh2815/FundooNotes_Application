using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using BusinessLayer.Interfaces;

namespace BusinessLayer.Services
{
    public class SmtpEmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public SmtpEmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var smtp = _configuration.GetSection("SmtpSettings");

            var host = smtp["Host"];
            var username = smtp["Username"];

            if (string.IsNullOrEmpty(host))
                throw new Exception("SMTP Host is NULL. Check appsettings.json");


            using var client = new SmtpClient
            {
                Host = smtp["Host"],

                Port = int.Parse(smtp["Port"]),
                EnableSsl = true,
                Credentials = new NetworkCredential(
                    smtp["Username"],
                    smtp["Password"]
                )
            };

            var mail = new MailMessage(
                smtp["Username"],
                to,
                subject,
                body
            )
            {
                IsBodyHtml = true
            };

            await client.SendMailAsync(mail);
        }
    }
}

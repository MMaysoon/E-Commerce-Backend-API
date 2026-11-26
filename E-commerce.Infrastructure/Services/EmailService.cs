
using E_commerce.Application.Contracts;
using E_commerce.Infrastructure.Settings;
using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace E_commerce.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(
            IOptions<EmailSettings> emailSettings,
            ILogger<EmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        public async Task SendEmailAsync(
            string email,
            string subject,
            string body,
            bool isBodyHtml = true)
        {
            try
            {
                var message = new MimeMessage();

                message.From.Add(
                    new MailboxAddress(
                        _emailSettings.FromName,
                        _emailSettings.FromEmail
                    )
                );

                message.To.Add(
                    MailboxAddress.Parse(email)
                );

                message.Subject = subject;

                var bodyBuilder = new BodyBuilder();

                if (isBodyHtml)
                    bodyBuilder.HtmlBody = body;
                else
                    bodyBuilder.TextBody = body;

                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();

                await client.ConnectAsync(
                    _emailSettings.Host,
                    _emailSettings.Port,
                    SecureSocketOptions.SslOnConnect
                );

                await client.AuthenticateAsync(
                    _emailSettings.UserName,
                    _emailSettings.Password
                );

                await client.SendAsync(message);

                await client.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to send email."
                );

                throw;
            }
        }
    }
}

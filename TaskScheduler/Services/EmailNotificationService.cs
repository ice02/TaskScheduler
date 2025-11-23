using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using MimeKit;
using TaskScheduler.Models;

namespace TaskScheduler.Services;

public class EmailNotificationService
{
    private readonly SmtpSettings _smtpSettings;
    private readonly ILogger<EmailNotificationService> _logger;

    public EmailNotificationService(SmtpSettings smtpSettings, ILogger<EmailNotificationService> logger)
    {
        _smtpSettings = smtpSettings;
        _logger = logger;
    }

    public async Task SendErrorNotificationAsync(string subject, string body)
    {
        if (!_smtpSettings.Enabled)
        {
            _logger.LogDebug("Email notifications are disabled");
            return;
        }

        if (_smtpSettings.ToEmails == null || _smtpSettings.ToEmails.Count == 0)
        {
            _logger.LogWarning("No recipient email addresses configured");
            return;
        }

        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_smtpSettings.FromName, _smtpSettings.FromEmail));

            foreach (var toEmail in _smtpSettings.ToEmails)
            {
                message.To.Add(new MailboxAddress("", toEmail));
            }

            message.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                TextBody = body,
                HtmlBody = $"<pre>{body}</pre>"
            };

            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            
            await client.ConnectAsync(_smtpSettings.Host, _smtpSettings.Port, 
                _smtpSettings.UseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None);

            if (!string.IsNullOrEmpty(_smtpSettings.Username) && !string.IsNullOrEmpty(_smtpSettings.Password))
            {
                await client.AuthenticateAsync(_smtpSettings.Username, _smtpSettings.Password);
            }

            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Error notification email sent successfully to {Recipients}", 
                string.Join(", ", _smtpSettings.ToEmails));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send error notification email");
        }
    }
}

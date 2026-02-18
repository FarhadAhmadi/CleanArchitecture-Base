using System.Net;
using System.Net.Mail;
using Domain.Notifications;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Notifications;

internal sealed class EmailNotificationSender(
    NotificationOptions options,
    ILogger<EmailNotificationSender> logger) : INotificationChannelSender
{
    public NotificationChannel Channel => NotificationChannel.Email;

    public async Task<NotificationDispatchResult> SendAsync(
        NotificationMessage message,
        string recipient,
        CancellationToken cancellationToken)
    {
        NotificationEmailOptions email = options.Email;
        if (!email.Enabled)
        {
            return new NotificationDispatchResult(false, null, "Email sender is disabled.");
        }

        if (string.IsNullOrWhiteSpace(email.Host) || string.IsNullOrWhiteSpace(email.FromAddress))
        {
            return new NotificationDispatchResult(false, null, "Email configuration is incomplete.");
        }

        try
        {
            _ = new MailAddress(recipient);
        }
        catch (FormatException)
        {
            return new NotificationDispatchResult(false, null, "Recipient email is invalid.");
        }

        try
        {
            using MailMessage mail = new();
            mail.From = new MailAddress(email.FromAddress, email.FromName);
            mail.To.Add(recipient);
            mail.Subject = string.IsNullOrWhiteSpace(message.Subject) ? "(no-subject)" : message.Subject.Trim();
            mail.Body = message.Body ?? string.Empty;
            mail.IsBodyHtml = email.IsBodyHtml;

            using SmtpClient client = new(email.Host, Math.Max(1, email.Port))
            {
                EnableSsl = email.UseSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = string.IsNullOrWhiteSpace(email.UserName)
            };

            if (!string.IsNullOrWhiteSpace(email.UserName))
            {
                client.Credentials = new NetworkCredential(email.UserName, email.Password);
            }

            await client.SendMailAsync(mail, cancellationToken);
            return new NotificationDispatchResult(true, $"smtp-{Guid.NewGuid():N}", null);
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                ex,
                "Email notification sending failed. NotificationId={NotificationId} RecipientHash={RecipientHash}",
                message.Id,
                message.RecipientHash ?? "n/a");
            return new NotificationDispatchResult(false, null, ex.Message);
        }
    }
}

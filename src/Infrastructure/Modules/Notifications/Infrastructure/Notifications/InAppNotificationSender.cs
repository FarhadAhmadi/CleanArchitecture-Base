using Domain.Notifications;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Notifications;

internal sealed class InAppNotificationSender(
    NotificationOptions options,
    ILogger<InAppNotificationSender> logger) : INotificationChannelSender
{
    public NotificationChannel Channel => NotificationChannel.InApp;

    public Task<NotificationDispatchResult> SendAsync(
        NotificationMessage message,
        string recipient,
        CancellationToken cancellationToken)
    {
        if (!options.InApp.Enabled)
        {
            return Task.FromResult(new NotificationDispatchResult(false, null, "InApp sender is disabled."));
        }

        if (string.IsNullOrWhiteSpace(recipient))
        {
            return Task.FromResult(new NotificationDispatchResult(false, null, "InApp recipient is required."));
        }

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "InApp notification delivered. NotificationId={NotificationId} RecipientHash={RecipientHash}",
                message.Id,
                message.RecipientHash ?? "n/a");
        }
        return Task.FromResult(new NotificationDispatchResult(true, $"inapp-{message.Id:N}", null));
    }
}

using Domain.Notifications;

namespace Infrastructure.Notifications;

public interface INotificationChannelSender
{
    NotificationChannel Channel { get; }

    Task<NotificationDispatchResult> SendAsync(
        NotificationMessage message,
        string recipient,
        CancellationToken cancellationToken);
}

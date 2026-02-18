namespace Domain.Notifications;

public enum NotificationStatus
{
    Pending = 1,
    Scheduled = 2,
    Sent = 3,
    Delivered = 4,
    Failed = 5,
    Cancelled = 6
}

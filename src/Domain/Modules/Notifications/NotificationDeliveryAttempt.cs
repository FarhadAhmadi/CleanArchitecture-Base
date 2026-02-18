using SharedKernel;

namespace Domain.Notifications;

public sealed class NotificationDeliveryAttempt : Entity
{
    public Guid NotificationId { get; set; }
    public NotificationChannel Channel { get; set; }
    public int AttemptNumber { get; set; }
    public bool IsSuccess { get; set; }
    public string? ProviderMessageId { get; set; }
    public string? Error { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

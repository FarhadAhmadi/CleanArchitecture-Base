using SharedKernel;

namespace Domain.Notifications;

public sealed class NotificationSchedule : Entity
{
    public Guid NotificationId { get; set; }
    public DateTime RunAtUtc { get; set; }
    public string? RuleName { get; set; }
    public bool IsCancelled { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

using SharedKernel;

namespace Domain.Notifications;

public sealed class NotificationMessage : Entity
{
    public Guid CreatedByUserId { get; set; }
    public NotificationChannel Channel { get; set; }
    public NotificationPriority Priority { get; set; }
    public NotificationStatus Status { get; set; }
    public string RecipientEncrypted { get; set; } = string.Empty;
    public string? RecipientHash { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string Language { get; set; } = "fa-IR";
    public Guid? TemplateId { get; set; }
    public int RetryCount { get; set; }
    public int MaxRetryCount { get; set; } = 5;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? ScheduledAtUtc { get; set; }
    public DateTime? SentAtUtc { get; set; }
    public DateTime? DeliveredAtUtc { get; set; }
    public DateTime? NextRetryAtUtc { get; set; }
    public string? LastError { get; set; }
    public bool IsArchived { get; set; }
    public DateTime? ArchivedAtUtc { get; set; }
}

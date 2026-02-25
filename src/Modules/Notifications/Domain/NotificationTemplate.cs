using SharedKernel;

namespace Domain.Notifications;

public sealed class NotificationTemplate : Entity
{
    public string Name { get; set; } = string.Empty;
    public NotificationChannel Channel { get; set; }
    public string Language { get; set; } = "fa-IR";
    public string SubjectTemplate { get; set; } = string.Empty;
    public string BodyTemplate { get; set; } = string.Empty;
    public int Version { get; set; } = 1;
    public bool IsDeleted { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}

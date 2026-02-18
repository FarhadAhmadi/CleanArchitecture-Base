using SharedKernel;

namespace Domain.Notifications;

public sealed class NotificationTemplateRevision : Entity
{
    public Guid TemplateId { get; set; }
    public int Version { get; set; }
    public string SubjectTemplate { get; set; } = string.Empty;
    public string BodyTemplate { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public Guid? ChangedByUserId { get; set; }
}

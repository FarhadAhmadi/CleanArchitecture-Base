using SharedKernel;

namespace Domain.Notifications;

public sealed class NotificationPermissionEntry : Entity
{
    public Guid NotificationId { get; set; }
    public string SubjectType { get; set; } = string.Empty;
    public string SubjectValue { get; set; } = string.Empty;
    public bool CanRead { get; set; }
    public bool CanManage { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

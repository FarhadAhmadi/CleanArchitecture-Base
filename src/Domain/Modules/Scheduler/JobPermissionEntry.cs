using SharedKernel;

namespace Domain.Modules.Scheduler;

public sealed class JobPermissionEntry : Entity
{
    public Guid JobId { get; set; }
    public string SubjectType { get; set; } = string.Empty;
    public string SubjectValue { get; set; } = string.Empty;
    public bool CanRead { get; set; }
    public bool CanManage { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}


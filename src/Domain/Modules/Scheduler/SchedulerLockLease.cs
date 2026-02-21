namespace Domain.Modules.Scheduler;

public sealed class SchedulerLockLease
{
    public string LockName { get; set; } = string.Empty;
    public string OwnerNodeId { get; set; } = string.Empty;
    public DateTime AcquiredAtUtc { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
}

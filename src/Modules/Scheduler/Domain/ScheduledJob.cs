using Domain.Scheduler;
using SharedKernel;

namespace Domain.Modules.Scheduler;

public sealed class ScheduledJob : Entity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public JobType Type { get; set; } = JobType.GenericNoOp;
    public string? PayloadJson { get; set; }
    public JobStatus Status { get; set; } = JobStatus.Active;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? LastRunAtUtc { get; set; }
    public JobExecutionStatus? LastExecutionStatus { get; set; }
    public int MaxRetryAttempts { get; set; } = 3;
    public int RetryBackoffSeconds { get; set; } = 15;
    public int MaxExecutionSeconds { get; set; } = 120;
    public int MaxConsecutiveFailures { get; set; } = 5;
    public int ConsecutiveFailures { get; set; }
    public bool IsQuarantined { get; set; }
    public DateTime? QuarantinedUntilUtc { get; set; }
    public DateTime? LastFailureAtUtc { get; set; }
    public string? DeadLetterReason { get; set; }
}

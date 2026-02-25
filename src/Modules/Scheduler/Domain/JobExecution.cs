using Domain.Scheduler;
using SharedKernel;

namespace Domain.Modules.Scheduler;

public sealed class JobExecution : Entity
{
    public Guid JobId { get; set; }
    public JobExecutionStatus Status { get; set; }
    public string TriggeredBy { get; set; } = "system";
    public string? NodeId { get; set; }
    public DateTime ScheduledAtUtc { get; set; }
    public DateTime StartedAtUtc { get; set; }
    public DateTime? FinishedAtUtc { get; set; }
    public int DurationMs { get; set; }
    public int Attempt { get; set; }
    public int MaxAttempts { get; set; }
    public bool IsReplay { get; set; }
    public bool IsDeadLetter { get; set; }
    public string? DeadLetterReason { get; set; }
    public string? PayloadSnapshotJson { get; set; }
    public string? Error { get; set; }
}

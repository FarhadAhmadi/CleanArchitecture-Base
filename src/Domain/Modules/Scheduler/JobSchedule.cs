using Domain.Scheduler;
using SharedKernel;

namespace Domain.Modules.Scheduler;

public sealed class JobSchedule : Entity
{
    public Guid JobId { get; set; }
    public ScheduleType Type { get; set; }
    public string? CronExpression { get; set; }
    public int? IntervalSeconds { get; set; }
    public DateTime? OneTimeAtUtc { get; set; }
    public DateTime? StartAtUtc { get; set; }
    public DateTime? EndAtUtc { get; set; }
    public DateTime? NextRunAtUtc { get; set; }
    public bool IsEnabled { get; set; } = true;
    public MisfirePolicy MisfirePolicy { get; set; } = MisfirePolicy.FireNow;
    public int MaxCatchUpRuns { get; set; } = 1;
    public int RetryAttempt { get; set; }
    public DateTime? LastMisfireAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}

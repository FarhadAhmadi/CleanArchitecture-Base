namespace Domain.Scheduler;

public enum JobExecutionStatus
{
    Scheduled = 1,
    Running = 2,
    Succeeded = 3,
    Failed = 4,
    Canceled = 5,
    TimedOut = 6,
    Skipped = 7,
    DeadLettered = 8
}

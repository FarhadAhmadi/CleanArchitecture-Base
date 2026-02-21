namespace Domain.Scheduler;

public enum JobType
{
    GenericNoOp = 1,
    CleanupOldSchedulerExecutions = 2,
    NotificationDispatchProbe = 3
}


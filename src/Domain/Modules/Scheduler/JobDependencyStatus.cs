namespace Domain.Modules.Scheduler;

public sealed class JobDependencyStatus
{
    public Guid JobId { get; set; }
    public Guid DependsOnJobId { get; set; }
    public bool LastDependencySucceeded { get; set; }
}


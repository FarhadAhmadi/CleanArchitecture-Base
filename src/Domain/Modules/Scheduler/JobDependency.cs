using SharedKernel;

namespace Domain.Modules.Scheduler;

public sealed class JobDependency : Entity
{
    public Guid JobId { get; set; }
    public Guid DependsOnJobId { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}


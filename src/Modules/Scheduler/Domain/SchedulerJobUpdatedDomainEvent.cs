using Domain.Scheduler;
using SharedKernel;

namespace Domain.Modules.Scheduler;

public sealed record SchedulerJobUpdatedDomainEvent(
    Guid JobId,
    string Name,
    JobType Type) : IDomainEvent;

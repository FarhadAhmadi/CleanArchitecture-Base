using Domain.Scheduler;
using SharedKernel;

namespace Domain.Modules.Scheduler;

public sealed record SchedulerJobStatusChangedDomainEvent(
    Guid JobId,
    JobStatus Status,
    string Reason) : IDomainEvent;

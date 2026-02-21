using Domain.Modules.Scheduler;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Application.Scheduler;

internal sealed class SchedulerJobCreatedDomainEventHandler(
    ILogger<SchedulerJobCreatedDomainEventHandler> logger)
    : IDomainEventHandler<SchedulerJobCreatedDomainEvent>
{
    public Task Handle(SchedulerJobCreatedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Scheduler job created domain event. JobId={JobId} Name={Name} Type={Type}",
                domainEvent.JobId,
                domainEvent.Name,
                domainEvent.Type);
        }

        return Task.CompletedTask;
    }
}

internal sealed class SchedulerJobUpdatedDomainEventHandler(
    ILogger<SchedulerJobUpdatedDomainEventHandler> logger)
    : IDomainEventHandler<SchedulerJobUpdatedDomainEvent>
{
    public Task Handle(SchedulerJobUpdatedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Scheduler job updated domain event. JobId={JobId} Name={Name} Type={Type}",
                domainEvent.JobId,
                domainEvent.Name,
                domainEvent.Type);
        }

        return Task.CompletedTask;
    }
}

internal sealed class SchedulerJobStatusChangedDomainEventHandler(
    ILogger<SchedulerJobStatusChangedDomainEventHandler> logger)
    : IDomainEventHandler<SchedulerJobStatusChangedDomainEvent>
{
    public Task Handle(SchedulerJobStatusChangedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Scheduler job status changed domain event. JobId={JobId} Status={Status} Reason={Reason}",
                domainEvent.JobId,
                domainEvent.Status,
                domainEvent.Reason);
        }

        return Task.CompletedTask;
    }
}

using Application.Abstractions.Logging;
using Domain.Logging;
using SharedKernel;

namespace Application.Logging.Alerts;

internal sealed class AlertIncidentCreatedDomainEventHandler(
    IAlertIncidentDispatchScheduler scheduler)
    : IDomainEventHandler<AlertIncidentCreatedDomainEvent>
{
    public async Task Handle(AlertIncidentCreatedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        await scheduler.ScheduleAsync(domainEvent.IncidentId, cancellationToken);
    }
}

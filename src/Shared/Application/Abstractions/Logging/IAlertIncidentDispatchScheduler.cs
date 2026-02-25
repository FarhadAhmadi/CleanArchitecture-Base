namespace Application.Abstractions.Logging;

public interface IAlertIncidentDispatchScheduler
{
    Task ScheduleAsync(Guid incidentId, CancellationToken cancellationToken);
}

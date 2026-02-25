using Application.Abstractions.Logging;

namespace Infrastructure.Logging;

internal sealed class AlertIncidentDispatchScheduler(IAlertDispatchQueue queue) : IAlertIncidentDispatchScheduler
{
    public Task ScheduleAsync(Guid incidentId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        bool queued = queue.TryEnqueue(incidentId);
        if (!queued)
        {
            throw new InvalidOperationException($"Failed to enqueue alert incident '{incidentId:N}'.");
        }

        return Task.CompletedTask;
    }
}

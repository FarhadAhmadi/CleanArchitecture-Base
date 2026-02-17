namespace Infrastructure.Logging;

public interface IAlertDispatchQueue
{
    bool TryEnqueue(Guid incidentId);
    ValueTask<Guid> DequeueAsync(CancellationToken cancellationToken);
    int ApproximateCount { get; }
}

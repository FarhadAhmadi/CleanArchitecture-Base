using Domain.Logging;

namespace Infrastructure.Logging;

public interface ILogIngestionQueue
{
    bool TryEnqueue(LogEvent item);
    ValueTask<LogEvent> DequeueAsync(CancellationToken cancellationToken);
    int ApproximateCount { get; }
    long DroppedCount { get; }
}

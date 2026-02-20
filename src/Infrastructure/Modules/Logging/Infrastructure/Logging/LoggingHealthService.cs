namespace Infrastructure.Logging;

public sealed class LoggingHealthService(
    ILogIngestionQueue ingestionQueue,
    IAlertDispatchQueue alertQueue) : ILoggingHealthService
{
    public object GetHealth()
    {
        return new
        {
            status = "ok",
            ingestionQueueDepth = ingestionQueue.ApproximateCount,
            ingestionDropped = ingestionQueue.DroppedCount,
            alertQueueDepth = alertQueue.ApproximateCount,
            timestampUtc = DateTime.UtcNow
        };
    }
}

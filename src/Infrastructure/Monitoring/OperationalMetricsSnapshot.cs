namespace Infrastructure.Monitoring;

public sealed record OperationalMetricsSnapshot(
    int IngestionQueueDepth,
    long IngestionDropped,
    int AlertQueueDepth,
    int TotalLogEvents,
    int CorruptedLogEvents,
    int OutboxPending,
    int OutboxFailed,
    DateTime TimestampUtc);

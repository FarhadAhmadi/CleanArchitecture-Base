namespace Application.Abstractions.Observability;

public interface IOperationalMetricsService
{
    Task<OperationalMetricsSnapshot> GetSnapshotAsync(CancellationToken cancellationToken);
}

public sealed record OperationalMetricsSnapshot(
    int IngestionQueueDepth,
    long IngestionDropped,
    int AlertQueueDepth,
    int TotalLogEvents,
    int CorruptedLogEvents,
    int OutboxPending,
    int OutboxFailed,
    DateTime TimestampUtc);

public sealed class OperationalSloOptions
{
    public const string SectionName = "OperationalSlo";

    public double MaxCorruptedLogRatePercent { get; init; } = 0.5;
    public int MaxOutboxPending { get; init; } = 1000;
    public int MaxOutboxFailed { get; init; }
    public int MaxIngestionQueueDepth { get; init; } = 10000;
}

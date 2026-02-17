namespace Web.Api.Infrastructure;

internal sealed class OperationalSloOptions
{
    public const string SectionName = "OperationalSlo";

    public double MaxCorruptedLogRatePercent { get; init; } = 0.5;
    public int MaxOutboxPending { get; init; } = 1000;
    public int MaxOutboxFailed { get; init; }
    public int MaxIngestionQueueDepth { get; init; } = 10000;
}

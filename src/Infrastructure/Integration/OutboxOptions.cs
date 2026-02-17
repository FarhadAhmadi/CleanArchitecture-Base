namespace Infrastructure.Integration;

public sealed class OutboxOptions
{
    public const string SectionName = "Outbox";

    public int BatchSize { get; set; } = 100;
    public int PollingIntervalSeconds { get; set; } = 5;
    public int MaxRetryCount { get; set; } = 10;
}

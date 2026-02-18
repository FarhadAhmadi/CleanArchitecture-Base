namespace Infrastructure.Logging;

public sealed class LoggingOptions
{
    public const string SectionName = "LoggingPlatform";

    public int MaxBulkItems { get; init; } = 200;
    public int MaxPayloadSizeKb { get; init; } = 64;
    public int QueueCapacity { get; init; } = 50000;
    public int RetryMaxAttempts { get; init; } = 6;
    public int RetryInitialDelayMs { get; init; } = 200;
}

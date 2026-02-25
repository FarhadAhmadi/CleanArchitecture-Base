namespace Web.Api.Infrastructure;

internal sealed class IdempotencyOptions
{
    public const string SectionName = "Idempotency";

    public bool Enabled { get; init; } = true;
    public int ExpirationMinutes { get; init; } = 30;
    public int MaxResponseBodyBytes { get; init; } = 64 * 1024;
    public int CleanupIntervalMinutes { get; init; } = 10;
    public int CleanupBatchSize { get; init; } = 500;
}

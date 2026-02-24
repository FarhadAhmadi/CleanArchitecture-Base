namespace Infrastructure.Files;

public sealed class FileReconciliationOptions
{
    public const string SectionName = "FileReconciliation";

    public bool Enabled { get; init; } = true;
    public int PollingSeconds { get; init; } = 120;
    public int BatchSize { get; init; } = 100;
    public int PendingStaleMinutes { get; init; } = 30;
}

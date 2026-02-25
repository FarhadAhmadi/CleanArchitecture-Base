namespace Infrastructure.Files;

public sealed class FileCleanupOptions
{
    public const string SectionName = "FileCleanup";

    public bool Enabled { get; init; } = true;
    public int IntervalSeconds { get; init; } = 30;
    public int BatchSize { get; init; } = 100;
}

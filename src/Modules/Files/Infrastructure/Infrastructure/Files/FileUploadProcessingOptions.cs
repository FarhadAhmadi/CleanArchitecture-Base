namespace Infrastructure.Files;

public sealed class FileUploadProcessingOptions
{
    public const string SectionName = "FileUploadProcessing";

    public bool Enabled { get; init; } = true;
    public int PollingSeconds { get; init; } = 3;
    public int BatchSize { get; init; } = 20;
    public int MaxRetryCount { get; init; } = 6;
}

namespace Infrastructure.Files;

public sealed class FileStorageOptions
{
    public const string SectionName = "FileStorage";

    public bool Enabled { get; init; } = true;
    public string Endpoint { get; init; } = "localhost:9000";
    public string AccessKey { get; init; } = string.Empty;
    public string SecretKey { get; init; } = string.Empty;
    public string Bucket { get; init; } = "nextgen-files";
    public bool UseSsl { get; init; }
    public bool CreateBucketIfMissing { get; init; } = true;
    public int PresignedUrlExpiryMinutes { get; init; } = 15;
    public string AppLinkSigningKey { get; init; } = "change-me-file-link-key";
    public int AppLinkExpiryMinutes { get; init; } = 15;
    public string ObjectPrefix { get; init; } = "files";
    public string StagingObjectPrefix { get; init; } = "staging";
    public int RetryMaxAttempts { get; init; } = 3;
    public int RetryBaseDelayMs { get; init; } = 200;
    public int RequestTimeoutSeconds { get; init; } = 15;
    public int CircuitBreakerFailureRatioPercent { get; init; } = 50;
    public int CircuitBreakerSamplingSeconds { get; init; } = 30;
    public int CircuitBreakerMinimumThroughput { get; init; } = 20;
    public int CircuitBreakerBreakSeconds { get; init; } = 20;
    public bool EnableMissingObjectPlaceholder { get; init; } = true;
    public string MissingObjectPlaceholderSvg { get; init; } =
        "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"320\" height=\"240\" viewBox=\"0 0 320 240\"><rect width=\"320\" height=\"240\" fill=\"#f3f4f6\"/><text x=\"50%\" y=\"50%\" dominant-baseline=\"middle\" text-anchor=\"middle\" font-family=\"Segoe UI, sans-serif\" font-size=\"16\" fill=\"#6b7280\">File unavailable</text></svg>";
}

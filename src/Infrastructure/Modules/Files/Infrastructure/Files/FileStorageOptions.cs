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
    public string ObjectPrefix { get; init; } = "files";
}

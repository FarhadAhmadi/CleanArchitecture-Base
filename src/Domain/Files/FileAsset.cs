using SharedKernel;

namespace Domain.Files;

public sealed class FileAsset : Entity
{
    public Guid OwnerUserId { get; set; }
    public string ObjectKey { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string Extension { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public string Module { get; set; } = string.Empty;
    public string? Folder { get; set; }
    public string? Description { get; set; }
    public string Sha256 { get; set; } = string.Empty;
    public bool IsScanned { get; set; }
    public bool IsInfected { get; set; }
    public bool IsEncrypted { get; set; }
    public DateTime UploadedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }
}

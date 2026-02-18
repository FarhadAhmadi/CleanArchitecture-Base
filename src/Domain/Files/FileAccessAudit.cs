using SharedKernel;

namespace Domain.Files;

public sealed class FileAccessAudit : Entity
{
    public Guid FileId { get; set; }
    public Guid? UserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public DateTime TimestampUtc { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}

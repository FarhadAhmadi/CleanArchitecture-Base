using SharedKernel;

namespace Domain.Files;

public sealed class FilePermissionEntry : Entity
{
    public Guid FileId { get; set; }
    public string SubjectType { get; set; } = string.Empty;
    public string SubjectValue { get; set; } = string.Empty;
    public bool CanRead { get; set; }
    public bool CanWrite { get; set; }
    public bool CanDelete { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

using Microsoft.AspNetCore.Identity;

namespace Domain.Authorization;

public sealed class Role : IdentityRole<Guid>
{
    public Role()
    {
        Id = Guid.NewGuid();
    }

    public string? AuditCreatedBy { get; set; }
    public DateTime AuditCreatedAtUtc { get; set; }
    public string? AuditUpdatedBy { get; set; }
    public DateTime? AuditUpdatedAtUtc { get; set; }
}

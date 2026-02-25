namespace Application.Authorization.Roles;

public sealed class RoleCrudResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string NormalizedName { get; set; } = string.Empty;
    public int PermissionCount { get; set; }
    public int UserCount { get; set; }
    public bool IsSystemRole { get; set; }
    public string? AuditCreatedBy { get; set; }
    public DateTime AuditCreatedAtUtc { get; set; }
    public string? AuditUpdatedBy { get; set; }
    public DateTime? AuditUpdatedAtUtc { get; set; }
}

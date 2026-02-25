namespace Application.Users.Management;

public sealed record UserAdminResponse
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string? PhoneNumber { get; init; }
    public bool EmailConfirmed { get; init; }
    public bool PhoneNumberConfirmed { get; init; }
    public bool TwoFactorEnabled { get; init; }
    public bool LockoutEnabled { get; init; }
    public DateTime? LockoutEndUtc { get; init; }
    public int FailedLoginCount { get; init; }
    public string? AuditCreatedBy { get; init; }
    public DateTime AuditCreatedAtUtc { get; init; }
    public string? AuditUpdatedBy { get; init; }
    public DateTime? AuditUpdatedAtUtc { get; init; }
    public List<string> Roles { get; init; } = [];
}

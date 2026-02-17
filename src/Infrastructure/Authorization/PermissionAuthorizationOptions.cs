namespace Infrastructure.Authorization;

internal sealed class PermissionAuthorizationOptions
{
    public const string SectionName = "Authorization";

    public string PermissionClaimType { get; init; } = "permissions";

    public HashSet<string> DefaultAuthenticatedPermissions { get; init; } = [];

    public Dictionary<string, string[]> StaticUserPermissions { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    public Dictionary<string, string[]> StaticEmailPermissions { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    public Dictionary<string, string[]> RolePermissions { get; init; } = new(StringComparer.OrdinalIgnoreCase);
}

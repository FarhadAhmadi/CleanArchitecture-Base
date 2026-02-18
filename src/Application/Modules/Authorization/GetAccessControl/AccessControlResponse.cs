namespace Application.Authorization.GetAccessControl;

public sealed class AccessControlResponse
{
    public List<RoleResponse> Roles { get; set; } = [];
    public List<PermissionResponse> Permissions { get; set; } = [];
}

public sealed class RoleResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public List<string> Permissions { get; set; } = [];
}

public sealed class PermissionResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; }
    public string Description { get; set; }
}

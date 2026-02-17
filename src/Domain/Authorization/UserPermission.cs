namespace Domain.Authorization;

public sealed class UserPermission
{
    public Guid UserId { get; set; }
    public Guid PermissionId { get; set; }
}

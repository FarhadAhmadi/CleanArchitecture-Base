using Application.Abstractions.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Authorization;

internal sealed class PermissionProvider(IApplicationReadDbContext context)
{
    public async Task<HashSet<string>> GetForUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        List<string> directPermissions = await (
            from userPermission in context.UserPermissions
            join permission in context.Permissions on userPermission.PermissionId equals permission.Id
            where userPermission.UserId == userId
            select permission.Code)
            .ToListAsync(cancellationToken);

        List<string> rolePermissions = await (
            from userRole in context.UserRoles
            join rolePermission in context.RolePermissions on userRole.RoleId equals rolePermission.RoleId
            join permission in context.Permissions on rolePermission.PermissionId equals permission.Id
            where userRole.UserId == userId
            select permission.Code)
            .ToListAsync(cancellationToken);

        return [.. directPermissions.Concat(rolePermissions).Distinct(StringComparer.OrdinalIgnoreCase)];
    }
}

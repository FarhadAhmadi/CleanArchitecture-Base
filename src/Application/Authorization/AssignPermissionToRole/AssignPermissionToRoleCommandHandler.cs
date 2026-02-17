using Application.Abstractions.Authorization;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Authorization;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Authorization.AssignPermissionToRole;

internal sealed class AssignPermissionToRoleCommandHandler(
    IApplicationDbContext context,
    IPermissionCacheVersionService permissionCacheVersionService)
    : ICommandHandler<AssignPermissionToRoleCommand>
{
    public async Task<Result> Handle(AssignPermissionToRoleCommand command, CancellationToken cancellationToken)
    {
        Role? role = await context.Roles.SingleOrDefaultAsync(x => x.Name == command.RoleName, cancellationToken);
        if (role is null)
        {
            return Result.Failure(Error.NotFound("Roles.NotFound", $"Role '{command.RoleName}' was not found."));
        }

        Permission? permission = await context.Permissions.SingleOrDefaultAsync(
            x => x.Code == command.PermissionCode,
            cancellationToken);

        if (permission is null)
        {
            return Result.Failure(Error.NotFound("Permissions.NotFound", $"Permission '{command.PermissionCode}' was not found."));
        }

        bool alreadyAssigned = await context.RolePermissions.AnyAsync(
            x => x.RoleId == role.Id && x.PermissionId == permission.Id,
            cancellationToken);

        if (alreadyAssigned)
        {
            return Result.Success();
        }

        context.RolePermissions.Add(new RolePermission
        {
            RoleId = role.Id,
            PermissionId = permission.Id
        });

        await context.SaveChangesAsync(cancellationToken);
        await permissionCacheVersionService.BumpVersionAsync(cancellationToken);

        return Result.Success();
    }
}

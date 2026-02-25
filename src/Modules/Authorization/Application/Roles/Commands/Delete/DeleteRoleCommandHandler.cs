using Application.Abstractions.Authorization;
using Application.Abstractions.Messaging;
using Domain.Authorization;
using Microsoft.AspNetCore.Identity;
using SharedKernel;

namespace Application.Authorization.Roles;

internal sealed class DeleteRoleCommandHandler(
    RoleManager<Role> roleManager,
    IPermissionCacheVersionService permissionCacheVersionService)
    : ICommandHandler<DeleteRoleCommand>
{
    private static readonly HashSet<string> ProtectedRoleNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "admin",
        "user"
    };

    public async Task<Result> Handle(DeleteRoleCommand command, CancellationToken cancellationToken)
    {
        Role? role = await roleManager.FindByIdAsync(command.RoleId.ToString());
        if (role is null)
        {
            return Result.Failure(
                Error.NotFound("Roles.NotFound", $"Role '{command.RoleId}' was not found."));
        }

        string roleName = role.Name ?? string.Empty;
        if (ProtectedRoleNames.Contains(roleName))
        {
            return Result.Failure(
                Error.Conflict("Roles.Protected", $"Role '{roleName}' cannot be deleted."));
        }

        IdentityResult deleteResult = await roleManager.DeleteAsync(role);
        if (!deleteResult.Succeeded)
        {
            string description = string.Join("; ", deleteResult.Errors.Select(e => e.Description));
            return Result.Failure(Error.Problem("Roles.DeleteFailed", description));
        }

        await permissionCacheVersionService.BumpVersionAsync(cancellationToken);

        return Result.Success();
    }
}

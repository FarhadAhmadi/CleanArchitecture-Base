using Application.Abstractions.Authorization;
using Application.Abstractions.Messaging;
using Domain.Authorization;
using Microsoft.AspNetCore.Identity;
using SharedKernel;

namespace Application.Authorization.Roles;

internal sealed class UpdateRoleCommandHandler(
    RoleManager<Role> roleManager,
    IPermissionCacheVersionService permissionCacheVersionService)
    : ICommandHandler<UpdateRoleCommand, RoleCrudResponse>
{
    public async Task<Result<RoleCrudResponse>> Handle(UpdateRoleCommand command, CancellationToken cancellationToken)
    {
        Role? role = await roleManager.FindByIdAsync(command.RoleId.ToString());
        if (role is null)
        {
            return Result.Failure<RoleCrudResponse>(
                Error.NotFound("Roles.NotFound", $"Role '{command.RoleId}' was not found."));
        }

        Role? existingByName = await roleManager.FindByNameAsync(command.RoleName);
        if (existingByName is not null && existingByName.Id != role.Id)
        {
            return Result.Failure<RoleCrudResponse>(
                Error.Conflict("Roles.AlreadyExists", $"Role '{command.RoleName}' already exists."));
        }

        role.Name = command.RoleName;

        IdentityResult updateResult = await roleManager.UpdateAsync(role);
        if (!updateResult.Succeeded)
        {
            string description = string.Join("; ", updateResult.Errors.Select(e => e.Description));
            return Result.Failure<RoleCrudResponse>(Error.Problem("Roles.UpdateFailed", description));
        }

        await permissionCacheVersionService.BumpVersionAsync(cancellationToken);

        return new RoleCrudResponse
        {
            Id = role.Id,
            Name = role.Name ?? command.RoleName
        };
    }
}

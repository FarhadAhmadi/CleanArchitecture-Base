using Application.Abstractions.Authorization;
using Application.Abstractions.Messaging;
using Domain.Authorization;
using Microsoft.AspNetCore.Identity;
using SharedKernel;

namespace Application.Authorization.Roles;

internal sealed class CreateRoleCommandHandler(
    RoleManager<Role> roleManager,
    IPermissionCacheVersionService permissionCacheVersionService)
    : ICommandHandler<CreateRoleCommand, RoleCrudResponse>
{
    public async Task<Result<RoleCrudResponse>> Handle(CreateRoleCommand command, CancellationToken cancellationToken)
    {
        bool exists = await roleManager.RoleExistsAsync(command.RoleName);
        if (exists)
        {
            return Result.Failure<RoleCrudResponse>(
                Error.Conflict("Roles.AlreadyExists", $"Role '{command.RoleName}' already exists."));
        }

        Role role = new()
        {
            Name = command.RoleName
        };

        IdentityResult createResult = await roleManager.CreateAsync(role);
        if (!createResult.Succeeded)
        {
            string description = string.Join("; ", createResult.Errors.Select(e => e.Description));
            return Result.Failure<RoleCrudResponse>(Error.Problem("Roles.CreateFailed", description));
        }

        await permissionCacheVersionService.BumpVersionAsync(cancellationToken);

        return new RoleCrudResponse
        {
            Id = role.Id,
            Name = role.Name ?? command.RoleName
        };
    }
}

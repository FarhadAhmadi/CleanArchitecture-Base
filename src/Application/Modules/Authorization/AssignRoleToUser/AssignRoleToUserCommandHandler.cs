using Application.Abstractions.Authorization;
using Application.Abstractions.Messaging;
using Domain.Authorization;
using Domain.Users;
using Microsoft.AspNetCore.Identity;
using SharedKernel;

namespace Application.Authorization.AssignRoleToUser;

internal sealed class AssignRoleToUserCommandHandler(
    UserManager<User> userManager,
    RoleManager<Role> roleManager,
    IPermissionCacheVersionService permissionCacheVersionService)
    : ICommandHandler<AssignRoleToUserCommand>
{
    public async Task<Result> Handle(AssignRoleToUserCommand command, CancellationToken cancellationToken)
    {
        User? user = await userManager.FindByIdAsync(command.UserId.ToString());

        if (user is null)
        {
            return Result.Failure(UserErrors.NotFound(command.UserId));
        }

        Role? role = await roleManager.FindByNameAsync(command.RoleName);

        if (role is null)
        {
            return Result.Failure(Error.NotFound("Roles.NotFound", $"Role '{command.RoleName}' was not found."));
        }

        string roleName = role.Name ?? command.RoleName;

        if (await userManager.IsInRoleAsync(user, roleName))
        {
            return Result.Success();
        }

        IdentityResult addRoleResult = await userManager.AddToRoleAsync(user, roleName);

        if (!addRoleResult.Succeeded)
        {
            string description = string.Join("; ", addRoleResult.Errors.Select(e => e.Description));
            return Result.Failure(Error.Problem("Roles.AssignFailed", description));
        }

        await permissionCacheVersionService.BumpVersionAsync(cancellationToken);

        return Result.Success();
    }
}

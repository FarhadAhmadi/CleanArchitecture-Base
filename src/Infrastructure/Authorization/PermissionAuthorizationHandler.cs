using Microsoft.AspNetCore.Authorization;

namespace Infrastructure.Authorization;

internal sealed class PermissionAuthorizationHandler(PermissionProvider permissionProvider)
    : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated is not true)
        {
            return Task.CompletedTask;
        }

        HashSet<string> permissions = permissionProvider.GetForUser(context.User);

        if (permissions.Contains(requirement.Permission) || permissions.Contains("*"))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

using Infrastructure.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace Infrastructure.Authorization;

internal sealed class PermissionAuthorizationHandler(PermissionProvider permissionProvider)
    : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated is not true)
        {
            return;
        }

        Guid userId = context.User.GetUserId();

        HashSet<string> permissions = await permissionProvider.GetForUserAsync(userId, CancellationToken.None);

        if (permissions.Contains(requirement.Permission, StringComparer.OrdinalIgnoreCase) ||
            permissions.Contains("*", StringComparer.OrdinalIgnoreCase))
        {
            context.Succeed(requirement);
        }
    }
}

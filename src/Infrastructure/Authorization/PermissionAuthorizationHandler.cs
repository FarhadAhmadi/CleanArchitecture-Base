using Application.Abstractions.Security;
using Infrastructure.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Authorization;

internal sealed class PermissionAuthorizationHandler(
    PermissionProvider permissionProvider,
    ISecurityEventLogger securityEventLogger)
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
            return;
        }

        securityEventLogger.AuthorizationDenied(
            requirement.Permission,
            context.User.Identity?.Name,
            (context.Resource as HttpContext)?.Connection.RemoteIpAddress?.ToString(),
            (context.Resource as HttpContext)?.TraceIdentifier);
    }
}

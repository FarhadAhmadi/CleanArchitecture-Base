using System.Security.Claims;
using Infrastructure.Authentication;

namespace Infrastructure.Authorization;

internal sealed class PermissionProvider(PermissionAuthorizationOptions options)
{
    public HashSet<string> GetForUser(ClaimsPrincipal user)
    {
        HashSet<string> permissions = new(StringComparer.OrdinalIgnoreCase);

        foreach (string permission in options.DefaultAuthenticatedPermissions)
        {
            permissions.Add(permission);
        }

        foreach (string permission in user.GetPermissions(options.PermissionClaimType))
        {
            permissions.Add(permission);
        }

        if (user.TryGetUserId(out Guid userId) &&
            options.StaticUserPermissions.TryGetValue(userId.ToString(), out string[]? userPermissions))
        {
            foreach (string permission in userPermissions)
            {
                if (!string.IsNullOrWhiteSpace(permission))
                {
                    permissions.Add(permission);
                }
            }
        }

        string? email = user.GetEmail();

        if (!string.IsNullOrWhiteSpace(email) &&
            options.StaticEmailPermissions.TryGetValue(email, out string[]? emailPermissions))
        {
            foreach (string permission in emailPermissions)
            {
                if (!string.IsNullOrWhiteSpace(permission))
                {
                    permissions.Add(permission);
                }
            }
        }

        foreach (string role in user.GetRoles())
        {
            if (!options.RolePermissions.TryGetValue(role, out string[]? rolePermissions))
            {
                continue;
            }

            foreach (string permission in rolePermissions)
            {
                if (!string.IsNullOrWhiteSpace(permission))
                {
                    permissions.Add(permission);
                }
            }
        }

        return permissions;
    }
}

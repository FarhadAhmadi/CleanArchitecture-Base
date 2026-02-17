using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Infrastructure.Authentication;

internal static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal? principal)
    {
        if (TryGetUserId(principal, out Guid userId))
        {
            return userId;
        }

        throw new ApplicationException("User id is unavailable");
    }

    public static bool TryGetUserId(this ClaimsPrincipal? principal, out Guid userId)
    {
        string? rawUserId =
            principal?.FindFirstValue(ClaimTypes.NameIdentifier) ??
            principal?.FindFirstValue(JwtRegisteredClaimNames.Sub);

        return Guid.TryParse(rawUserId, out userId);
    }

    public static string? GetEmail(this ClaimsPrincipal? principal) =>
        principal?.FindFirstValue(ClaimTypes.Email) ??
        principal?.FindFirstValue(JwtRegisteredClaimNames.Email);

    public static IEnumerable<string> GetRoles(this ClaimsPrincipal? principal)
    {
        if (principal is null)
        {
            return [];
        }

        return principal.FindAll(ClaimTypes.Role)
            .Select(c => c.Value)
            .Concat(principal.FindAll("role").Select(c => c.Value))
            .Where(role => !string.IsNullOrWhiteSpace(role))
            .Distinct(StringComparer.OrdinalIgnoreCase);
    }

    public static IEnumerable<string> GetPermissions(this ClaimsPrincipal? principal, string permissionClaimType)
    {
        if (principal is null)
        {
            return [];
        }

        return principal.FindAll(permissionClaimType)
            .Select(c => c.Value)
            .Concat(principal.FindAll("permission").Select(c => c.Value))
            .SelectMany(value => value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .Where(permission => !string.IsNullOrWhiteSpace(permission))
            .Distinct(StringComparer.OrdinalIgnoreCase);
    }
}

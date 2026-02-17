using System.Security.Claims;
using System.Text;
using Application.Abstractions.Authentication;
using Domain.Users;
using Infrastructure.Authorization;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Authentication;

internal sealed class TokenProvider(
    JwtOptions jwtOptions,
    PermissionAuthorizationOptions authorizationOptions) : ITokenProvider
{
    public string Create(User user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        List<Claim> claims =
        [
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
        ];

        foreach (string permission in ResolvePermissions(user))
        {
            claims.Add(new Claim(authorizationOptions.PermissionClaimType, permission));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(jwtOptions.ExpirationInMinutes),
            SigningCredentials = credentials,
            Issuer = jwtOptions.Issuer,
            Audience = jwtOptions.Audience
        };

        var handler = new JsonWebTokenHandler();

        return handler.CreateToken(tokenDescriptor);
    }

    private HashSet<string> ResolvePermissions(User user)
    {
        HashSet<string> permissions = new(StringComparer.OrdinalIgnoreCase);

        foreach (string permission in authorizationOptions.DefaultAuthenticatedPermissions)
        {
            permissions.Add(permission);
        }

        if (authorizationOptions.StaticUserPermissions.TryGetValue(user.Id.ToString(), out string[]? userPermissions))
        {
            foreach (string permission in userPermissions)
            {
                if (!string.IsNullOrWhiteSpace(permission))
                {
                    permissions.Add(permission);
                }
            }
        }

        if (authorizationOptions.StaticEmailPermissions.TryGetValue(user.Email, out string[]? emailPermissions))
        {
            foreach (string permission in emailPermissions)
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

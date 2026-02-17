using System.Security.Claims;
using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Security;
using Domain.Users;
using Infrastructure.Auditing;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Users;

internal sealed class OAuth : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("users/oauth/{provider}/start", Start)
            .WithTags(Tags.Users);

        app.MapGet("users/oauth/callback", Callback)
            .WithTags(Tags.Users);
    }

    private static IResult Start(string provider, string? returnUrl)
    {
        string? scheme = null;
        if (string.Equals(provider, "google", StringComparison.OrdinalIgnoreCase))
        {
            scheme = ExternalAuthSchemes.Google;
        }
        else if (string.Equals(provider, "meta", StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(provider, "facebook", StringComparison.OrdinalIgnoreCase))
        {
            scheme = ExternalAuthSchemes.Meta;
        }

        if (scheme is null)
        {
            return Results.BadRequest(new { error = "Unsupported provider." });
        }

        string callback = "/api/v1/users/oauth/callback";
        if (!string.IsNullOrWhiteSpace(returnUrl))
        {
            callback += $"?returnUrl={Uri.EscapeDataString(returnUrl)}";
        }

        AuthenticationProperties properties = new()
        {
            RedirectUri = callback
        };
        properties.Items["provider"] = scheme;

        return Results.Challenge(properties, [scheme]);
    }

    private static async Task<IResult> Callback(
        HttpContext httpContext,
        IApplicationDbContext context,
        ITokenProvider tokenProvider,
        IRefreshTokenProvider refreshTokenProvider,
        ITokenLifetimeProvider tokenLifetimeProvider,
        IAuditTrailService auditTrailService,
        ISecurityEventLogger securityEventLogger,
        CancellationToken cancellationToken)
    {
        AuthenticateResult result = await httpContext.AuthenticateAsync(ExternalAuthSchemes.ExternalCookie);
        if (!result.Succeeded || result.Principal is null)
        {
            securityEventLogger.AuthenticationFailed(
                "ExternalOAuthCallbackFailed",
                null,
                httpContext.Connection.RemoteIpAddress?.ToString(),
                httpContext.TraceIdentifier);
            return Results.BadRequest(new { error = "External authentication failed." });
        }

        ClaimsPrincipal principal = result.Principal;
        string? provider = result.Properties?.Items.TryGetValue("provider", out string? p) == true ? p : null;
        string? email = principal.FindFirstValue(ClaimTypes.Email) ??
                        principal.FindFirstValue("email");
        string? providerUserId = principal.FindFirstValue(ClaimTypes.NameIdentifier) ??
                                 principal.FindFirstValue("sub");

        if (string.IsNullOrWhiteSpace(provider) ||
            string.IsNullOrWhiteSpace(providerUserId) ||
            string.IsNullOrWhiteSpace(email))
        {
            securityEventLogger.AuthenticationFailed(
                "ExternalClaimsIncomplete",
                email,
                httpContext.Connection.RemoteIpAddress?.ToString(),
                httpContext.TraceIdentifier);
            return Results.BadRequest(new { error = "Provider claims are incomplete." });
        }

        email = InputSanitizer.SanitizeEmail(email) ?? email;

        User? user = await (
            from external in context.UserExternalLogins
            join candidate in context.Users on external.UserId equals candidate.Id
            where external.Provider == provider && external.ProviderUserId == providerUserId
            select candidate)
            .SingleOrDefaultAsync(cancellationToken);

        if (user is null)
        {
            user = await context.Users.SingleOrDefaultAsync(x => x.Email == email, cancellationToken);
            if (user is null)
            {
                user = new User
                {
                    Id = Guid.NewGuid(),
                    Email = email,
                    FirstName = principal.FindFirstValue(ClaimTypes.GivenName) ?? "External",
                    LastName = principal.FindFirstValue(ClaimTypes.Surname) ?? "User",
                    PasswordHash = Guid.NewGuid().ToString("N")
                };
                context.Users.Add(user);
            }

            bool exists = await context.UserExternalLogins.AnyAsync(
                x => x.Provider == provider && x.ProviderUserId == providerUserId,
                cancellationToken);
            if (!exists)
            {
                context.UserExternalLogins.Add(new UserExternalLogin
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    Provider = provider,
                    ProviderUserId = providerUserId,
                    Email = email,
                    LinkedAtUtc = DateTime.UtcNow
                });
            }
        }

        user.FailedLoginCount = 0;
        user.LockoutEndUtc = null;

        string accessToken = tokenProvider.Create(user);
        string refreshToken = refreshTokenProvider.Generate();
        string refreshTokenHash = refreshTokenProvider.Hash(refreshToken);
        DateTime now = DateTime.UtcNow;
        DateTime refreshExpiresAt = now.AddDays(tokenLifetimeProvider.RefreshTokenExpirationInDays);

        context.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = refreshTokenHash,
            CreatedAtUtc = now,
            ExpiresAtUtc = refreshExpiresAt
        });

        await context.SaveChangesAsync(cancellationToken);
        await httpContext.SignOutAsync(ExternalAuthSchemes.ExternalCookie);

        await auditTrailService.RecordAsync(
            new AuditRecordRequest(
                user.Id.ToString("N"),
                "auth.oauth.success",
                "ExternalOAuth",
                user.Id.ToString("N"),
                $"{{\"provider\":\"{provider}\"}}"),
            cancellationToken);

        securityEventLogger.AuthenticationSucceeded(
            user.Id.ToString("N"),
            httpContext.Connection.RemoteIpAddress?.ToString(),
            httpContext.TraceIdentifier,
            $"oauth:{provider}");

        return Results.Ok(new TokenResponse(
            accessToken,
            refreshToken,
            now.AddMinutes(tokenLifetimeProvider.AccessTokenExpirationInMinutes),
            refreshExpiresAt));
    }
}

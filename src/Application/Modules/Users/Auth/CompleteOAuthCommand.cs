using System.Security.Claims;
using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Domain.Users;
using Infrastructure.Auditing;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;

namespace Application.Users.Auth;

public sealed record CompleteOAuthCommand(HttpContext HttpContext) : ICommand<IResult>;

internal sealed class CompleteOAuthCommandHandler(
    IApplicationDbContext context,
    ITokenProvider tokenProvider,
    IRefreshTokenProvider refreshTokenProvider,
    ITokenLifetimeProvider tokenLifetimeProvider,
    IAuditTrailService auditTrailService,
    ISecurityEventLogger securityEventLogger) : ResultWrappingCommandHandler<CompleteOAuthCommand>
{
    protected override async Task<IResult> HandleCore(CompleteOAuthCommand command, CancellationToken cancellationToken)
    {
        AuthenticateResult result = await command.HttpContext.AuthenticateAsync("ExternalCookie");
        if (!result.Succeeded || result.Principal is null)
        {
            securityEventLogger.AuthenticationFailed(
                "ExternalOAuthCallbackFailed",
                null,
                command.HttpContext.Connection.RemoteIpAddress?.ToString(),
                command.HttpContext.TraceIdentifier);
            return Results.BadRequest(new { error = "External authentication failed." });
        }

        ClaimsPrincipal principal = result.Principal;
        string? provider = result.Properties?.Items.TryGetValue("provider", out string? p) == true ? p : null;
        string? email = principal.FindFirstValue(ClaimTypes.Email) ?? principal.FindFirstValue("email");
        string? providerUserId = principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? principal.FindFirstValue("sub");

        if (string.IsNullOrWhiteSpace(provider) || string.IsNullOrWhiteSpace(providerUserId) || string.IsNullOrWhiteSpace(email))
        {
            securityEventLogger.AuthenticationFailed(
                "ExternalClaimsIncomplete",
                email,
                command.HttpContext.Connection.RemoteIpAddress?.ToString(),
                command.HttpContext.TraceIdentifier);
            return Results.BadRequest(new { error = "Provider claims are incomplete." });
        }

        email = email.Trim();

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
        await command.HttpContext.SignOutAsync("ExternalCookie");

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
            command.HttpContext.Connection.RemoteIpAddress?.ToString(),
            command.HttpContext.TraceIdentifier,
            $"oauth:{provider}");

        return Results.Ok(new TokenResponse(
            accessToken,
            refreshToken,
            now.AddMinutes(tokenLifetimeProvider.AccessTokenExpirationInMinutes),
            refreshExpiresAt));
    }
}

using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Users.Login;

internal sealed class LoginUserCommandHandler(
    IApplicationDbContext context,
    IPasswordHasher passwordHasher,
    ITokenProvider tokenProvider,
    IRefreshTokenProvider refreshTokenProvider,
    ITokenLifetimeProvider tokenLifetimeProvider,
    AuthSecurityOptions authSecurityOptions,
    ISecurityEventLogger securityEventLogger) : ICommandHandler<LoginUserCommand, TokenResponse>
{
    public async Task<Result<TokenResponse>> Handle(LoginUserCommand command, CancellationToken cancellationToken)
    {
        User? user = await context.Users
            .SingleOrDefaultAsync(u => u.Email == command.Email, cancellationToken);

        if (user is null)
        {
            securityEventLogger.AuthenticationFailed("UserNotFound", command.Email, null, null);
            return Result.Failure<TokenResponse>(UserErrors.NotFoundByEmail);
        }

        if (user.LockoutEndUtc.HasValue && user.LockoutEndUtc.Value > DateTime.UtcNow)
        {
            securityEventLogger.AccountLocked(user.Id.ToString("N"), user.LockoutEndUtc.Value, null, null);
            return Result.Failure<TokenResponse>(UserErrors.NotFoundByEmail);
        }

        bool verified = passwordHasher.Verify(command.Password, user.PasswordHash);

        if (!verified)
        {
            user.FailedLoginCount++;
            if (user.FailedLoginCount >= Math.Max(1, authSecurityOptions.MaxFailedLoginAttempts))
            {
                user.LockoutEndUtc = DateTime.UtcNow.AddMinutes(Math.Max(1, authSecurityOptions.LockoutMinutes));
                securityEventLogger.AccountLocked(user.Id.ToString("N"), user.LockoutEndUtc.Value, null, null);
            }

            await context.SaveChangesAsync(cancellationToken);
            securityEventLogger.AuthenticationFailed("InvalidPassword", command.Email, null, null);
            return Result.Failure<TokenResponse>(UserErrors.NotFoundByEmail);
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
        securityEventLogger.AuthenticationSucceeded(user.Id.ToString("N"), null, null, "password");

        var response = new TokenResponse(
            accessToken,
            refreshToken,
            now.AddMinutes(tokenLifetimeProvider.AccessTokenExpirationInMinutes),
            refreshExpiresAt);

        return response;
    }
}

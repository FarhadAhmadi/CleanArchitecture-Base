using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Domain.Users;
using Microsoft.AspNetCore.Identity;
using SharedKernel;

namespace Application.Users.Login;

internal sealed class LoginUserCommandHandler(
    IApplicationDbContext context,
    UserManager<User> userManager,
    ITokenProvider tokenProvider,
    IRefreshTokenProvider refreshTokenProvider,
    ITokenLifetimeProvider tokenLifetimeProvider,
    AuthSecurityOptions authSecurityOptions,
    ISecurityEventLogger securityEventLogger) : ICommandHandler<LoginUserCommand, TokenResponse>
{
    public async Task<Result<TokenResponse>> Handle(LoginUserCommand command, CancellationToken cancellationToken)
    {
        User? user = await userManager.FindByEmailAsync(command.Email);

        if (user is null)
        {
            securityEventLogger.AuthenticationFailed("UserNotFound", command.Email, null, null);
            return Result.Failure<TokenResponse>(UserErrors.NotFoundByEmail);
        }

        if (await userManager.IsLockedOutAsync(user))
        {
            DateTime lockoutEnd = user.LockoutEnd?.UtcDateTime ?? DateTime.UtcNow.AddMinutes(Math.Max(1, authSecurityOptions.LockoutMinutes));
            securityEventLogger.AccountLocked(user.Id.ToString("N"), lockoutEnd, null, null);
            return Result.Failure<TokenResponse>(UserErrors.NotFoundByEmail);
        }

        bool verified = await userManager.CheckPasswordAsync(user, command.Password);

        if (!verified)
        {
            await userManager.AccessFailedAsync(user);

            if (await userManager.IsLockedOutAsync(user))
            {
                DateTime lockoutEnd = user.LockoutEnd?.UtcDateTime ?? DateTime.UtcNow.AddMinutes(Math.Max(1, authSecurityOptions.LockoutMinutes));
                securityEventLogger.AccountLocked(user.Id.ToString("N"), lockoutEnd, null, null);
            }

            securityEventLogger.AuthenticationFailed("InvalidPassword", command.Email, null, null);
            return Result.Failure<TokenResponse>(UserErrors.NotFoundByEmail);
        }

        await userManager.ResetAccessFailedCountAsync(user);
        await userManager.SetLockoutEndDateAsync(user, null);

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

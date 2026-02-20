using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Users.Tokens;

internal sealed class RefreshTokenCommandHandler(
    IApplicationDbContext context,
    ITokenProvider tokenProvider,
    IRefreshTokenProvider refreshTokenProvider,
    ITokenLifetimeProvider tokenLifetimeProvider,
    ISecurityEventLogger securityEventLogger) : ICommandHandler<RefreshTokenCommand, TokenResponse>
{
    public async Task<Result<TokenResponse>> Handle(RefreshTokenCommand command, CancellationToken cancellationToken)
    {
        string refreshTokenHash = refreshTokenProvider.Hash(command.RefreshToken);

        RefreshToken? existing = await context.RefreshTokens
            .SingleOrDefaultAsync(x => x.TokenHash == refreshTokenHash, cancellationToken);

        if (existing is null)
        {
            securityEventLogger.AuthenticationFailed("RefreshTokenInvalid", null, null, null);
            return Result.Failure<TokenResponse>(UserErrors.InvalidRefreshToken);
        }

        if (existing.RevokedAtUtc.HasValue)
        {
            if (!string.IsNullOrWhiteSpace(existing.ReplacedByTokenHash))
            {
                await RevokeActiveUserRefreshTokensAsync(existing.UserId, "Refresh token reuse detected", cancellationToken);
                securityEventLogger.AuthenticationFailed("RefreshTokenReuseDetected", existing.UserId.ToString("N"), null, null);
                return Result.Failure<TokenResponse>(UserErrors.RefreshTokenReuseDetected);
            }

            securityEventLogger.AuthenticationFailed("RefreshTokenRevoked", existing.UserId.ToString("N"), null, null);
            return Result.Failure<TokenResponse>(UserErrors.RevokedRefreshToken);
        }

        if (existing.ExpiresAtUtc <= DateTime.UtcNow)
        {
            securityEventLogger.AuthenticationFailed("RefreshTokenExpired", existing.UserId.ToString("N"), null, null);
            return Result.Failure<TokenResponse>(UserErrors.ExpiredRefreshToken);
        }

        User? user = await context.Users
            .SingleOrDefaultAsync(u => u.Id == existing.UserId, cancellationToken);

        if (user is null)
        {
            securityEventLogger.AuthenticationFailed("RefreshUserNotFound", existing.UserId.ToString("N"), null, null);
            return Result.Failure<TokenResponse>(UserErrors.NotFound(existing.UserId));
        }

        string accessToken = tokenProvider.Create(user);

        string newRefreshToken = refreshTokenProvider.Generate();
        string newRefreshTokenHash = refreshTokenProvider.Hash(newRefreshToken);
        DateTime now = DateTime.UtcNow;
        DateTime refreshExpiresAt = now.AddDays(tokenLifetimeProvider.RefreshTokenExpirationInDays);

        existing.RevokedAtUtc = now;
        existing.ReplacedByTokenHash = newRefreshTokenHash;
        existing.RevokedReason = "Rotated";

        context.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = newRefreshTokenHash,
            CreatedAtUtc = now,
            ExpiresAtUtc = refreshExpiresAt
        });

        await context.SaveChangesAsync(cancellationToken);
        securityEventLogger.AuthenticationSucceeded(user.Id.ToString("N"), null, null, "refresh-token");

        return new TokenResponse(
            accessToken,
            newRefreshToken,
            now.AddMinutes(tokenLifetimeProvider.AccessTokenExpirationInMinutes),
            refreshExpiresAt);
    }

    private async Task RevokeActiveUserRefreshTokensAsync(Guid userId, string reason, CancellationToken cancellationToken)
    {
        DateTime nowUtc = DateTime.UtcNow;
        List<RefreshToken> activeTokens = await context.RefreshTokens
            .Where(x => x.UserId == userId && x.RevokedAtUtc == null && x.ExpiresAtUtc > nowUtc)
            .ToListAsync(cancellationToken);

        foreach (RefreshToken token in activeTokens)
        {
            token.RevokedAtUtc = nowUtc;
            token.RevokedReason = reason;
        }

        if (activeTokens.Count > 0)
        {
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}

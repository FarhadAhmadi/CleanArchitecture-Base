using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Users.Tokens;

internal sealed class RefreshTokenCommandHandler(
    IApplicationDbContext context,
    ITokenProvider tokenProvider,
    IRefreshTokenProvider refreshTokenProvider,
    ITokenLifetimeProvider tokenLifetimeProvider) : ICommandHandler<RefreshTokenCommand, TokenResponse>
{
    public async Task<Result<TokenResponse>> Handle(RefreshTokenCommand command, CancellationToken cancellationToken)
    {
        string refreshTokenHash = refreshTokenProvider.Hash(command.RefreshToken);

        RefreshToken? existing = await context.RefreshTokens
            .SingleOrDefaultAsync(x => x.TokenHash == refreshTokenHash, cancellationToken);

        if (existing is null)
        {
            return Result.Failure<TokenResponse>(UserErrors.InvalidRefreshToken);
        }

        if (existing.RevokedAtUtc.HasValue)
        {
            return Result.Failure<TokenResponse>(UserErrors.RevokedRefreshToken);
        }

        if (existing.ExpiresAtUtc <= DateTime.UtcNow)
        {
            return Result.Failure<TokenResponse>(UserErrors.ExpiredRefreshToken);
        }

        User? user = await context.Users
            .SingleOrDefaultAsync(u => u.Id == existing.UserId, cancellationToken);

        if (user is null)
        {
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

        return new TokenResponse(
            accessToken,
            newRefreshToken,
            now.AddMinutes(tokenLifetimeProvider.AccessTokenExpirationInMinutes),
            refreshExpiresAt);
    }
}

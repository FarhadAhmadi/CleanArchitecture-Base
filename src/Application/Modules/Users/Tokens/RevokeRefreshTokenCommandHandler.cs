using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Users.Tokens;

internal sealed class RevokeRefreshTokenCommandHandler(
    IApplicationDbContext context,
    IRefreshTokenProvider refreshTokenProvider,
    ISecurityEventLogger securityEventLogger) : ICommandHandler<RevokeRefreshTokenCommand>
{
    public async Task<Result> Handle(RevokeRefreshTokenCommand command, CancellationToken cancellationToken)
    {
        string refreshTokenHash = refreshTokenProvider.Hash(command.RefreshToken);

        Domain.Users.RefreshToken? existing = await context.RefreshTokens
            .SingleOrDefaultAsync(x => x.TokenHash == refreshTokenHash, cancellationToken);

        if (existing is null)
        {
            securityEventLogger.AuthenticationFailed("RevokeRefreshTokenInvalid", null, null, null);
            return Result.Failure(Domain.Users.UserErrors.InvalidRefreshToken);
        }

        if (existing.RevokedAtUtc.HasValue)
        {
            return Result.Success();
        }

        existing.RevokedAtUtc = DateTime.UtcNow;
        existing.RevokedReason = "Revoked by user";

        await context.SaveChangesAsync(cancellationToken);
        securityEventLogger.AuthenticationSucceeded(existing.UserId.ToString("N"), null, null, "refresh-revoke");

        return Result.Success();
    }
}

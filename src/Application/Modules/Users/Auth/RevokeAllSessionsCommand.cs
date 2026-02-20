using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Infrastructure.Auditing;
using Microsoft.EntityFrameworkCore;

namespace Application.Users.Auth;

public sealed record RevokeAllSessionsCommand : ICommand<IResult>;

internal sealed class RevokeAllSessionsCommandHandler(
    IUserContext userContext,
    IApplicationDbContext dbContext,
    IAuditTrailService auditTrailService) : ResultWrappingCommandHandler<RevokeAllSessionsCommand>
{
    protected override async Task<IResult> HandleCore(RevokeAllSessionsCommand command, CancellationToken cancellationToken)
    {
        _ = command;

        DateTime nowUtc = DateTime.UtcNow;
        List<Domain.Users.RefreshToken> activeTokens = await dbContext.RefreshTokens
            .Where(x => x.UserId == userContext.UserId && x.RevokedAtUtc == null && x.ExpiresAtUtc > nowUtc)
            .ToListAsync(cancellationToken);

        foreach (Domain.Users.RefreshToken token in activeTokens)
        {
            token.RevokedAtUtc = nowUtc;
            token.RevokedReason = "Revoked all sessions";
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        await auditTrailService.RecordAsync(
            new AuditRecordRequest(
                userContext.UserId.ToString("N"),
                "auth.sessions.revoke-all",
                "AuthSession",
                userContext.UserId.ToString("N"),
                $"{{\"revokedCount\":{activeTokens.Count}}}"),
            cancellationToken);

        return Results.Ok(new { revoked = activeTokens.Count });
    }
}

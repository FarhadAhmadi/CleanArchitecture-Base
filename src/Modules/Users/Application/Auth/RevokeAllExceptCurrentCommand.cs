using Application.Abstractions.Authentication;
using Application.Abstractions.Auditing;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Application.Users.Auth;

public sealed record RevokeAllExceptCurrentCommand : ICommand<IResult>;

internal sealed class RevokeAllExceptCurrentCommandHandler(
    IUserContext userContext,
    IUsersWriteDbContext dbContext,
    IAuditTrailService auditTrailService) : ResultWrappingCommandHandler<RevokeAllExceptCurrentCommand>
{
    protected override async Task<IResult> HandleCore(
        RevokeAllExceptCurrentCommand command,
        CancellationToken cancellationToken)
    {
        _ = command;

        DateTime nowUtc = DateTime.UtcNow;
        List<RefreshToken> activeTokens = await dbContext.RefreshTokens
            .Where(x => x.UserId == userContext.UserId && x.RevokedAtUtc == null && x.ExpiresAtUtc > nowUtc)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        Guid? currentSessionId = activeTokens.FirstOrDefault()?.Id;
        var tokensToRevoke = activeTokens
            .Where(x => !currentSessionId.HasValue || x.Id != currentSessionId.Value)
            .ToList();

        foreach (RefreshToken token in tokensToRevoke)
        {
            token.RevokedAtUtc = nowUtc;
            token.RevokedReason = "Revoked all sessions except current";
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        await auditTrailService.RecordAsync(
            new AuditRecordRequest(
                userContext.UserId.ToString("N"),
                "auth.sessions.revoke-all-except-current",
                "AuthSession",
                userContext.UserId.ToString("N"),
                $"{{\"revokedCount\":{tokensToRevoke.Count}}}"),
            cancellationToken);

        return Results.Ok(new { revoked = tokensToRevoke.Count });
    }
}

using Application.Abstractions.Authentication;
using Application.Abstractions.Auditing;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Users.Auth;

public sealed record RevokeSessionByIdCommand(Guid SessionId) : ICommand;

internal sealed class RevokeSessionByIdCommandHandler(
    IUserContext userContext,
    IUsersWriteDbContext dbContext,
    IAuditTrailService auditTrailService) : ICommandHandler<RevokeSessionByIdCommand>
{
    public async Task<Result> Handle(RevokeSessionByIdCommand command, CancellationToken cancellationToken)
    {
        RefreshToken? token = await dbContext.RefreshTokens
            .FirstOrDefaultAsync(
                x => x.Id == command.SessionId && x.UserId == userContext.UserId,
                cancellationToken);

        if (token is null)
        {
            return Result.Failure(UserErrors.InvalidRefreshToken);
        }

        if (token.RevokedAtUtc is null)
        {
            token.RevokedAtUtc = DateTime.UtcNow;
            token.RevokedReason = "Revoked by session id";
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        await auditTrailService.RecordAsync(
            new AuditRecordRequest(
                userContext.UserId.ToString("N"),
                "auth.sessions.revoke-by-id",
                "AuthSession",
                command.SessionId.ToString("N"),
                "{}"),
            cancellationToken);

        return Result.Success();
    }
}

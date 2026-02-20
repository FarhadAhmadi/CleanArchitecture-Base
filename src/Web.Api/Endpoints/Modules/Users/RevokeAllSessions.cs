using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Infrastructure.Auditing;
using Microsoft.EntityFrameworkCore;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Users;

internal sealed class RevokeAllSessions : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("users/sessions/revoke-all", async (
            IUserContext userContext,
            IApplicationDbContext dbContext,
            IAuditTrailService auditTrailService,
            CancellationToken cancellationToken) =>
        {
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
        })
        .WithTags(Tags.Users)
        .RequireAuthorization();
    }
}

using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Users.Auth;

public sealed record UserSessionResponse(
    Guid Id,
    string? IpAddress,
    string? Device,
    DateTime CreatedAtUtc,
    DateTime ExpiresAtUtc,
    bool Current);

public sealed record GetUserSessionsQuery : IQuery<List<UserSessionResponse>>;

internal sealed class GetUserSessionsQueryHandler(
    IUserContext userContext,
    IUsersReadDbContext dbContext) : IQueryHandler<GetUserSessionsQuery, List<UserSessionResponse>>
{
    public async Task<Result<List<UserSessionResponse>>> Handle(
        GetUserSessionsQuery query,
        CancellationToken cancellationToken)
    {
        _ = query;

        DateTime nowUtc = DateTime.UtcNow;
        List<RefreshToken> activeTokens = await dbContext.RefreshTokens
            .Where(x => x.UserId == userContext.UserId && x.RevokedAtUtc == null && x.ExpiresAtUtc > nowUtc)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        Guid? currentSessionId = activeTokens.FirstOrDefault()?.Id;

        var sessions = activeTokens
            .Select(token => new UserSessionResponse(
                token.Id,
                null,
                null,
                token.CreatedAtUtc,
                token.ExpiresAtUtc,
                currentSessionId.HasValue && token.Id == currentSessionId.Value))
            .ToList();

        return sessions;
    }
}

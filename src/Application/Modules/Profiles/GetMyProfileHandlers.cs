using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Profiles;
using Microsoft.EntityFrameworkCore;

namespace Application.Profiles;

public sealed record GetMyProfileQuery : IQuery<IResult>;
internal sealed class GetMyProfileQueryHandler(
    IUserContext userContext,
    IApplicationReadDbContext readContext) : ResultWrappingQueryHandler<GetMyProfileQuery>
{
    protected override async Task<IResult> HandleCore(GetMyProfileQuery query, CancellationToken cancellationToken)
    {
        _ = query;
        return await GetAsync(userContext, readContext, cancellationToken);
    }

    private static async Task<IResult> GetAsync(
        IUserContext userContext,
        IApplicationReadDbContext readContext,
        CancellationToken cancellationToken)
    {
        UserProfile? profile = await readContext.UserProfiles
            .SingleOrDefaultAsync(x => x.UserId == userContext.UserId, cancellationToken);

        return profile is null
            ? Results.NotFound()
            : Results.Ok(ProfileEndpointCommon.ToPrivateResponse(profile));
    }
}







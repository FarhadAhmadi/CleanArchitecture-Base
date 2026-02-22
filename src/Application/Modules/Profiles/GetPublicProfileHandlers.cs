using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Profiles;
using Microsoft.EntityFrameworkCore;

namespace Application.Profiles;

public sealed record GetPublicProfileQuery(Guid UserId) : IQuery<IResult>;
internal sealed class GetPublicProfileQueryHandler(IProfilesReadDbContext readContext) : ResultWrappingQueryHandler<GetPublicProfileQuery>
{
    protected override async Task<IResult> HandleCore(GetPublicProfileQuery query, CancellationToken cancellationToken) =>
        await GetAsync(query.UserId, readContext, cancellationToken);

    private static async Task<IResult> GetAsync(
        Guid userId,
        IProfilesReadDbContext readContext,
        CancellationToken cancellationToken)
    {
        UserProfile? profile = await readContext.UserProfiles
            .SingleOrDefaultAsync(x => x.UserId == userId, cancellationToken);

        if (profile is null || !profile.IsProfilePublic)
        {
            return Results.NotFound();
        }

        return Results.Ok(ProfileEndpointCommon.ToPublicResponse(profile));
    }
}









using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Profiles;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Application.Shared;

namespace Application.Profiles;

public sealed record GetMyProfileMusicQuery : IQuery<IResult>;
internal sealed class GetMyProfileMusicQueryHandler(
    IUserContext userContext,
    IApplicationReadDbContext readContext) : ResultWrappingQueryHandler<GetMyProfileMusicQuery>
{
    protected override async Task<IResult> HandleCore(GetMyProfileMusicQuery query, CancellationToken cancellationToken)
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

        if (profile is null)
        {
            return Results.NotFound();
        }

        return Results.Ok(new
        {
            profile.FavoriteMusicTitle,
            profile.FavoriteMusicArtist,
            profile.FavoriteMusicFileId
        });
    }
}






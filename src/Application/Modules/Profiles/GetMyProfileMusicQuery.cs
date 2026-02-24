using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Files;
using Domain.Profiles;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Application.Shared;

namespace Application.Profiles;

public sealed record GetMyProfileMusicQuery : IQuery<IResult>;
internal sealed class GetMyProfileMusicQueryHandler(
    IUserContext userContext,
    IProfilesReadDbContext readContext) : ResultWrappingQueryHandler<GetMyProfileMusicQuery>
{
    protected override async Task<IResult> HandleCore(GetMyProfileMusicQuery query, CancellationToken cancellationToken)
    {
        _ = query;
        return await GetAsync(userContext, readContext, cancellationToken);
    }

    private static async Task<IResult> GetAsync(
        IUserContext userContext,
        IProfilesReadDbContext readContext,
        CancellationToken cancellationToken)
    {
        UserProfile? profile = await readContext.UserProfiles
            .SingleOrDefaultAsync(x => x.UserId == userContext.UserId, cancellationToken);

        if (profile is null)
        {
            return Results.NotFound();
        }

        string? musicStorageStatus = await ResolveStorageStatusAsync(profile.FavoriteMusicFileId, readContext, cancellationToken);
        Guid? musicFileId = string.Equals(musicStorageStatus, FileStorageStatus.Available.ToString(), StringComparison.Ordinal)
            ? profile.FavoriteMusicFileId
            : null;

        return Results.Ok(new
        {
            profile.FavoriteMusicTitle,
            profile.FavoriteMusicArtist,
            FavoriteMusicFileId = musicFileId,
            musicStorageStatus
        });
    }

    private static async Task<string?> ResolveStorageStatusAsync(
        Guid? fileId,
        IProfilesReadDbContext readContext,
        CancellationToken cancellationToken)
    {
        if (!fileId.HasValue)
        {
            return null;
        }

        FileStorageStatus? status = await readContext.FileAssets
            .Where(x => x.Id == fileId.Value && !x.IsDeleted)
            .Select(x => (FileStorageStatus?)x.StorageStatus)
            .SingleOrDefaultAsync(cancellationToken);

        return status?.ToString() ?? FileStorageStatus.Missing.ToString();
    }
}








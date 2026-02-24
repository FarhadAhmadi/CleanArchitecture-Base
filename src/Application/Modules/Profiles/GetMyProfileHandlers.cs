using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Files;
using Domain.Profiles;
using Microsoft.EntityFrameworkCore;

namespace Application.Profiles;

public sealed record GetMyProfileQuery : IQuery<IResult>;
internal sealed class GetMyProfileQueryHandler(
    IUserContext userContext,
    IProfilesReadDbContext readContext) : ResultWrappingQueryHandler<GetMyProfileQuery>
{
    protected override async Task<IResult> HandleCore(GetMyProfileQuery query, CancellationToken cancellationToken)
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

        return profile is null
            ? Results.NotFound()
            : Results.Ok(ProfileEndpointCommon.ToPrivateResponse(
                profile,
                await ResolveStorageStatusAsync(profile.AvatarFileId, readContext, cancellationToken),
                await ResolveStorageStatusAsync(profile.FavoriteMusicFileId, readContext, cancellationToken)));
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









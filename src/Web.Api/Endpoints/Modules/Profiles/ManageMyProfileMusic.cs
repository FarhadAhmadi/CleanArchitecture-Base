using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Domain.Profiles;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Profiles;

internal sealed class ManageMyProfileMusic : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("profiles/me/music", GetAsync)
            .HasPermission(Permissions.ProfilesRead)
            .WithTags(Tags.Profiles);

        app.MapPut("profiles/me/music", UpdateAsync)
            .HasPermission(Permissions.ProfilesWrite)
            .WithTags(Tags.Profiles);
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

    private static async Task<IResult> UpdateAsync(
        UpdateProfileMusicRequest request,
        IUserContext userContext,
        IApplicationDbContext writeContext,
        IValidator<UpdateProfileMusicRequest> validator,
        CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToValidationProblem();
        }

        UserProfile? profile = await ProfileEndpointCommon.GetCurrentProfileForUpdateAsync(
            userContext.UserId,
            writeContext,
            cancellationToken);

        if (profile is null)
        {
            return Results.NotFound();
        }

        bool fileOwnedByUser = await writeContext.FileAssets
            .AsNoTracking()
            .AnyAsync(
                x => x.Id == request.MusicFileId!.Value &&
                     !x.IsDeleted &&
                     x.OwnerUserId == userContext.UserId,
                cancellationToken);

        if (!fileOwnedByUser)
        {
            return Results.BadRequest(new { message = "musicFileId is invalid or not owned by user." });
        }

        profile.FavoriteMusicTitle = InputSanitizer.SanitizeText(request.MusicTitle, 200);
        profile.FavoriteMusicArtist = InputSanitizer.SanitizeText(request.MusicArtist, 200);
        profile.FavoriteMusicFileId = request.MusicFileId;
        profile.LastProfileUpdateAtUtc = DateTime.UtcNow;
        profile.ProfileCompletenessScore = ProfileEndpointCommon.ComputeCompleteness(profile);
        profile.Raise(new UserProfileChangedDomainEvent(profile.Id, profile.UserId, "Music", profile.ProfileCompletenessScore));

        await writeContext.SaveChangesAsync(cancellationToken);
        return Results.Ok(new
        {
            profile.FavoriteMusicTitle,
            profile.FavoriteMusicArtist,
            profile.FavoriteMusicFileId,
            profile.ProfileCompletenessScore
        });
    }
}

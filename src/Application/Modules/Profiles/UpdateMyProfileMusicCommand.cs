using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Profiles;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Application.Shared;

namespace Application.Profiles;

public sealed record UpdateMyProfileMusicCommand(UpdateProfileMusicRequest Request) : ICommand<IResult>;
internal sealed class UpdateMyProfileMusicCommandHandler(
    IUserContext userContext,
    IApplicationDbContext writeContext,
    IValidator<UpdateProfileMusicRequest> validator) : ResultWrappingCommandHandler<UpdateMyProfileMusicCommand>
{
    protected override async Task<IResult> HandleCore(UpdateMyProfileMusicCommand command, CancellationToken cancellationToken) =>
        await UpdateAsync(command.Request, userContext, writeContext, validator, cancellationToken);

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






using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Profiles;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Application.Shared;

namespace Application.Profiles;

public sealed record UpdateMyProfileMusicCommand(string? MusicTitle, string? MusicArtist, Guid? MusicFileId) : ICommand<IResult>;

internal sealed class UpdateMyProfileMusicCommandValidator : AbstractValidator<UpdateMyProfileMusicCommand>
{
    public UpdateMyProfileMusicCommandValidator()
    {
        RuleFor(x => x.MusicTitle).MaximumLength(200).When(x => !string.IsNullOrWhiteSpace(x.MusicTitle));
        RuleFor(x => x.MusicArtist).MaximumLength(200).When(x => !string.IsNullOrWhiteSpace(x.MusicArtist));
    }
}

internal sealed class UpdateMyProfileMusicCommandHandler(
    IUserContext userContext,
    IApplicationDbContext writeContext,
    IValidator<UpdateMyProfileMusicCommand> validator) : ResultWrappingCommandHandler<UpdateMyProfileMusicCommand>
{
    protected override async Task<IResult> HandleCore(UpdateMyProfileMusicCommand command, CancellationToken cancellationToken) =>
        await UpdateAsync(command, userContext, writeContext, validator, cancellationToken);

    private static async Task<IResult> UpdateAsync(
        UpdateMyProfileMusicCommand request,
        IUserContext userContext,
        IApplicationDbContext writeContext,
        IValidator<UpdateMyProfileMusicCommand> validator,
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






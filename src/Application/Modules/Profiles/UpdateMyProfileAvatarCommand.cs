using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Files;
using Domain.Profiles;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;

namespace Application.Profiles;

public sealed record UpdateMyProfileAvatarCommand(Guid? AvatarFileId) : ICommand<IResult>;

internal sealed class UpdateMyProfileAvatarCommandValidator : AbstractValidator<UpdateMyProfileAvatarCommand>
{
    public UpdateMyProfileAvatarCommandValidator()
    {
        RuleFor(x => x.AvatarFileId).NotNull();
    }
}

internal sealed class UpdateMyProfileAvatarCommandHandler(
    IUserContext userContext,
    IProfilesWriteDbContext writeContext,
    IValidator<UpdateMyProfileAvatarCommand> validator) : ResultWrappingCommandHandler<UpdateMyProfileAvatarCommand>
{
    protected override async Task<IResult> HandleCore(UpdateMyProfileAvatarCommand command, CancellationToken cancellationToken) =>
        await UpdateAsync(command, userContext, writeContext, validator, cancellationToken);

    private static async Task<IResult> UpdateAsync(
        UpdateMyProfileAvatarCommand request,
        IUserContext userContext,
        IProfilesWriteDbContext writeContext,
        IValidator<UpdateMyProfileAvatarCommand> validator,
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

        FileAsset? avatarFile = await writeContext.FileAssets
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == request.AvatarFileId!.Value && !x.IsDeleted, cancellationToken);

        if (avatarFile is null)
        {
            return Results.BadRequest(new { code = "Profiles.AvatarFileNotFound", message = "avatarFileId was not found." });
        }

        if (avatarFile.OwnerUserId != userContext.UserId)
        {
            return Results.BadRequest(new { code = "Profiles.AvatarFileNotOwned", message = "avatarFileId is not owned by user." });
        }

        if (avatarFile.StorageStatus == FileStorageStatus.Pending)
        {
            return Results.Conflict(new { code = "Profiles.AvatarFilePending", message = "avatarFileId is still processing." });
        }

        if (avatarFile.StorageStatus != FileStorageStatus.Available)
        {
            return Results.BadRequest(new
            {
                code = "Profiles.AvatarFileUnavailable",
                message = "avatarFileId is unavailable.",
                storageStatus = avatarFile.StorageStatus.ToString()
            });
        }

        profile.AvatarFileId = request.AvatarFileId;
        profile.AvatarUrl = null;
        profile.LastProfileUpdateAtUtc = DateTime.UtcNow;
        profile.ProfileCompletenessScore = ProfileEndpointCommon.ComputeCompleteness(profile);
        profile.Raise(new UserProfileChangedDomainEvent(profile.Id, profile.UserId, "Avatar", profile.ProfileCompletenessScore));

        await writeContext.SaveChangesAsync(cancellationToken);
        return Results.Ok(ProfileEndpointCommon.ToPrivateResponse(profile));
    }
}








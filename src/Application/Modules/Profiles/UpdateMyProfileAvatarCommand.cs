using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Profiles;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;

namespace Application.Profiles;

public sealed record UpdateMyProfileAvatarCommand(Guid? AvatarFileId) : ICommand<IResult>;

internal sealed class UpdateMyProfileAvatarCommandValidator : AbstractValidator<UpdateMyProfileAvatarCommand>
{
    public UpdateMyProfileAvatarCommandValidator() { }
}

internal sealed class UpdateMyProfileAvatarCommandHandler(
    IUserContext userContext,
    IApplicationDbContext writeContext,
    IValidator<UpdateMyProfileAvatarCommand> validator) : ResultWrappingCommandHandler<UpdateMyProfileAvatarCommand>
{
    protected override async Task<IResult> HandleCore(UpdateMyProfileAvatarCommand command, CancellationToken cancellationToken) =>
        await UpdateAsync(command, userContext, writeContext, validator, cancellationToken);

    private static async Task<IResult> UpdateAsync(
        UpdateMyProfileAvatarCommand request,
        IUserContext userContext,
        IApplicationDbContext writeContext,
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

        bool fileOwnedByUser = await writeContext.FileAssets
            .AsNoTracking()
            .AnyAsync(
                x => x.Id == request.AvatarFileId!.Value &&
                     !x.IsDeleted &&
                     x.OwnerUserId == userContext.UserId,
                cancellationToken);

        if (!fileOwnedByUser)
        {
            return Results.BadRequest(new { message = "avatarFileId is invalid or not owned by user." });
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






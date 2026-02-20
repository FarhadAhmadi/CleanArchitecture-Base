using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Domain.Profiles;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Web.Api.Extensions;

namespace Web.Api.Endpoints.Profiles;

internal sealed class UpdateMyProfileAvatar : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("profiles/me/avatar", UpdateAsync)
            .HasPermission(Permissions.ProfilesWrite)
            .WithTags(Tags.Profiles);

        app.MapDelete("profiles/me/avatar", DeleteAsync)
            .HasPermission(Permissions.ProfilesWrite)
            .WithTags(Tags.Profiles);
    }

    private static async Task<IResult> UpdateAsync(
        UpdateProfileAvatarRequest request,
        IUserContext userContext,
        IApplicationDbContext writeContext,
        IValidator<UpdateProfileAvatarRequest> validator,
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

    private static async Task<IResult> DeleteAsync(
        IUserContext userContext,
        IApplicationDbContext writeContext,
        CancellationToken cancellationToken)
    {
        UserProfile? profile = await ProfileEndpointCommon.GetCurrentProfileForUpdateAsync(
            userContext.UserId,
            writeContext,
            cancellationToken);

        if (profile is null)
        {
            return Results.NotFound();
        }

        profile.AvatarFileId = null;
        profile.AvatarUrl = null;
        profile.LastProfileUpdateAtUtc = DateTime.UtcNow;
        profile.ProfileCompletenessScore = ProfileEndpointCommon.ComputeCompleteness(profile);
        profile.Raise(new UserProfileChangedDomainEvent(profile.Id, profile.UserId, "Avatar", profile.ProfileCompletenessScore));

        await writeContext.SaveChangesAsync(cancellationToken);
        return Results.Ok(ProfileEndpointCommon.ToPrivateResponse(profile));
    }
}

using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Domain.Profiles;
using FluentValidation;
using FluentValidation.Results;
using Web.Api.Extensions;

namespace Web.Api.Endpoints.Profiles;

internal sealed class UpdateMyProfileSocialLinks : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("profiles/me/social-links", UpdateAsync)
            .HasPermission(Permissions.ProfilesWrite)
            .WithTags(Tags.Profiles);
    }

    private static async Task<IResult> UpdateAsync(
        UpdateProfileSocialLinksRequest request,
        IUserContext userContext,
        IApplicationDbContext writeContext,
        IValidator<UpdateProfileSocialLinksRequest> validator,
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

        profile.SocialLinksJson = ProfileEndpointCommon.BuildSocialLinksJson(request.Links);
        profile.LastProfileUpdateAtUtc = DateTime.UtcNow;
        profile.ProfileCompletenessScore = ProfileEndpointCommon.ComputeCompleteness(profile);
        profile.Raise(new UserProfileChangedDomainEvent(profile.Id, profile.UserId, "SocialLinks", profile.ProfileCompletenessScore));

        await writeContext.SaveChangesAsync(cancellationToken);
        return Results.Ok(ProfileEndpointCommon.ToPrivateResponse(profile));
    }
}

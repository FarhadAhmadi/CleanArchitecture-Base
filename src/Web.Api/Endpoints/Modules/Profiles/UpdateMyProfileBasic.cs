using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Domain.Profiles;
using FluentValidation;
using FluentValidation.Results;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Profiles;

internal sealed class UpdateMyProfileBasic : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("profiles/me/basic", UpdateAsync)
            .HasPermission(Permissions.ProfilesWrite)
            .WithTags(Tags.Profiles);
    }

    private static async Task<IResult> UpdateAsync(
        UpdateProfileBasicRequest request,
        IUserContext userContext,
        IApplicationDbContext writeContext,
        IValidator<UpdateProfileBasicRequest> validator,
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

        profile.DisplayName = InputSanitizer.SanitizeText(request.DisplayName, 160) ?? profile.DisplayName;
        profile.Bio = InputSanitizer.SanitizeText(request.Bio, 1200);
        profile.DateOfBirth = request.DateOfBirth;
        profile.Gender = InputSanitizer.SanitizeIdentifier(request.Gender, 32);
        profile.Location = InputSanitizer.SanitizeText(request.Location, 200);
        profile.LastProfileUpdateAtUtc = DateTime.UtcNow;
        profile.ProfileCompletenessScore = ProfileEndpointCommon.ComputeCompleteness(profile);
        profile.Raise(new UserProfileChangedDomainEvent(profile.Id, profile.UserId, "Basic", profile.ProfileCompletenessScore));

        await writeContext.SaveChangesAsync(cancellationToken);
        return Results.Ok(ProfileEndpointCommon.ToPrivateResponse(profile));
    }
}

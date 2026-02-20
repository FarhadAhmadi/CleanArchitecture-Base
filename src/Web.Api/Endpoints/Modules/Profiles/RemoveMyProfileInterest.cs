using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Domain.Profiles;
using FluentValidation;
using FluentValidation.Results;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Profiles;

internal sealed class RemoveMyProfileInterest : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("profiles/me/interests/{interest}", RemoveAsync)
            .HasPermission(Permissions.ProfilesWrite)
            .WithTags(Tags.Profiles);
    }

    private static async Task<IResult> RemoveAsync(
        string interest,
        IUserContext userContext,
        IApplicationDbContext writeContext,
        IValidator<RemoveProfileInterestRequest> validator,
        CancellationToken cancellationToken)
    {
        RemoveProfileInterestRequest request = new(interest);
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

        string normalized = InputSanitizer.SanitizeIdentifier(request.Interest, 60) ?? string.Empty;
        HashSet<string> currentInterests = [.. ProfileEndpointCommon.ParseInterests(profile.InterestsCsv)];
        currentInterests.RemoveWhere(x => string.Equals(x, normalized, StringComparison.OrdinalIgnoreCase));

        profile.InterestsCsv = string.Join(',', currentInterests.OrderBy(x => x, StringComparer.OrdinalIgnoreCase));
        profile.LastProfileUpdateAtUtc = DateTime.UtcNow;
        profile.ProfileCompletenessScore = ProfileEndpointCommon.ComputeCompleteness(profile);
        profile.Raise(new UserProfileChangedDomainEvent(profile.Id, profile.UserId, "Interests", profile.ProfileCompletenessScore));

        await writeContext.SaveChangesAsync(cancellationToken);
        return Results.Ok(ProfileEndpointCommon.ToPrivateResponse(profile));
    }
}

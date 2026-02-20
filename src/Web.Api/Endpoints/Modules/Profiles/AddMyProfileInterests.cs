using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Domain.Profiles;
using FluentValidation;
using FluentValidation.Results;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Profiles;

internal sealed class AddMyProfileInterests : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("profiles/me/interests", AddAsync)
            .HasPermission(Permissions.ProfilesWrite)
            .WithTags(Tags.Profiles);
    }

    private static async Task<IResult> AddAsync(
        AddProfileInterestsRequest request,
        IUserContext userContext,
        IApplicationDbContext writeContext,
        IValidator<AddProfileInterestsRequest> validator,
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

        HashSet<string> currentInterests = [.. ProfileEndpointCommon.ParseInterests(profile.InterestsCsv)];
        foreach (string interest in InputSanitizer.SanitizeList(request.Interests ?? [], 60))
        {
            currentInterests.Add(interest);
        }

        profile.InterestsCsv = string.Join(',', currentInterests.OrderBy(x => x, StringComparer.OrdinalIgnoreCase));
        profile.LastProfileUpdateAtUtc = DateTime.UtcNow;
        profile.ProfileCompletenessScore = ProfileEndpointCommon.ComputeCompleteness(profile);
        profile.Raise(new UserProfileChangedDomainEvent(profile.Id, profile.UserId, "Interests", profile.ProfileCompletenessScore));

        await writeContext.SaveChangesAsync(cancellationToken);
        return Results.Ok(ProfileEndpointCommon.ToPrivateResponse(profile));
    }
}

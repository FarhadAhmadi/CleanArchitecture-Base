using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Domain.Profiles;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Profiles;

internal sealed class CreateMyProfile : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("profiles/me", CreateAsync)
            .HasPermission(Permissions.ProfilesWrite)
            .WithTags(Tags.Profiles);
    }

    private static async Task<IResult> CreateAsync(
        CreateProfileRequest request,
        IUserContext userContext,
        IApplicationDbContext writeContext,
        IValidator<CreateProfileRequest> validator,
        CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToValidationProblem();
        }

        bool userExists = await writeContext.Users
            .AsNoTracking()
            .AnyAsync(x => x.Id == userContext.UserId, cancellationToken);

        if (!userExists)
        {
            return Results.NotFound(new { message = "User not found." });
        }

        UserProfile? existing = await writeContext.UserProfiles
            .SingleOrDefaultAsync(x => x.UserId == userContext.UserId, cancellationToken);

        if (existing is not null)
        {
            return Results.Conflict(new { message = "Profile already exists." });
        }

        UserProfile profile = new()
        {
            Id = Guid.NewGuid(),
            UserId = userContext.UserId,
            DisplayName = InputSanitizer.SanitizeText(request.DisplayName, 160) ?? "User",
            PreferredLanguage = InputSanitizer.SanitizeIdentifier(request.PreferredLanguage, 16) ?? "fa-IR",
            IsProfilePublic = request.IsProfilePublic,
            InterestsCsv = string.Empty,
            SocialLinksJson = "{}",
            LastProfileUpdateAtUtc = DateTime.UtcNow
        };

        profile.ProfileCompletenessScore = ProfileEndpointCommon.ComputeCompleteness(profile);
        profile.Raise(new UserProfileChangedDomainEvent(profile.Id, profile.UserId, "Created", profile.ProfileCompletenessScore));

        writeContext.UserProfiles.Add(profile);
        await writeContext.SaveChangesAsync(cancellationToken);

        return Results.Ok(ProfileEndpointCommon.ToPrivateResponse(profile));
    }
}

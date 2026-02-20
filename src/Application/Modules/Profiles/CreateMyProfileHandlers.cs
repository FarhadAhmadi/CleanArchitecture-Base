using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Profiles;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Application.Shared;

namespace Application.Profiles;

public sealed record CreateMyProfileCommand(CreateProfileRequest Request) : ICommand<IResult>;
internal sealed class CreateMyProfileCommandHandler(
    IUserContext userContext,
    IApplicationDbContext writeContext,
    IValidator<CreateProfileRequest> validator) : ResultWrappingCommandHandler<CreateMyProfileCommand>
{
    protected override async Task<IResult> HandleCore(CreateMyProfileCommand command, CancellationToken cancellationToken) =>
        await CreateAsync(command.Request, userContext, writeContext, validator, cancellationToken);

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







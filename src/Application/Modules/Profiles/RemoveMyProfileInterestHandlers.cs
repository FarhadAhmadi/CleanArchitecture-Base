using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Profiles;
using FluentValidation;
using FluentValidation.Results;
using Application.Shared;

namespace Application.Profiles;

public sealed record RemoveMyProfileInterestCommand(string Interest) : ICommand<IResult>;
internal sealed class RemoveMyProfileInterestCommandHandler(
    IUserContext userContext,
    IApplicationDbContext writeContext,
    IValidator<RemoveProfileInterestRequest> validator) : ResultWrappingCommandHandler<RemoveMyProfileInterestCommand>
{
    protected override async Task<IResult> HandleCore(RemoveMyProfileInterestCommand command, CancellationToken cancellationToken) =>
        await RemoveAsync(command.Interest, userContext, writeContext, validator, cancellationToken);

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







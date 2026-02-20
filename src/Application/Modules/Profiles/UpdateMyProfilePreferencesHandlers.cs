using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Profiles;
using FluentValidation;
using FluentValidation.Results;
using Application.Shared;

namespace Application.Profiles;

public sealed record UpdateMyProfilePreferencesCommand(
    string? PreferredLanguage,
    bool ReceiveSecurityAlerts,
    bool ReceiveProductUpdates) : ICommand<IResult>;

internal sealed class UpdateMyProfilePreferencesCommandValidator : AbstractValidator<UpdateMyProfilePreferencesCommand>
{
    public UpdateMyProfilePreferencesCommandValidator()
    {
        RuleFor(x => x.PreferredLanguage).MaximumLength(16).When(x => !string.IsNullOrWhiteSpace(x.PreferredLanguage));
    }
}

internal sealed class UpdateMyProfilePreferencesCommandHandler(
    IUserContext userContext,
    IApplicationDbContext writeContext,
    IValidator<UpdateMyProfilePreferencesCommand> validator) : ResultWrappingCommandHandler<UpdateMyProfilePreferencesCommand>
{
    protected override async Task<IResult> HandleCore(UpdateMyProfilePreferencesCommand command, CancellationToken cancellationToken) =>
        await UpdateAsync(command, userContext, writeContext, validator, cancellationToken);

    private static async Task<IResult> UpdateAsync(
        UpdateMyProfilePreferencesCommand request,
        IUserContext userContext,
        IApplicationDbContext writeContext,
        IValidator<UpdateMyProfilePreferencesCommand> validator,
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

        profile.PreferredLanguage = InputSanitizer.SanitizeIdentifier(request.PreferredLanguage, 16) ?? "fa-IR";
        profile.ReceiveSecurityAlerts = request.ReceiveSecurityAlerts;
        profile.ReceiveProductUpdates = request.ReceiveProductUpdates;
        profile.LastProfileUpdateAtUtc = DateTime.UtcNow;
        profile.ProfileCompletenessScore = ProfileEndpointCommon.ComputeCompleteness(profile);
        profile.Raise(new UserProfileChangedDomainEvent(profile.Id, profile.UserId, "Preferences", profile.ProfileCompletenessScore));

        await writeContext.SaveChangesAsync(cancellationToken);
        return Results.Ok(ProfileEndpointCommon.ToPrivateResponse(profile));
    }
}







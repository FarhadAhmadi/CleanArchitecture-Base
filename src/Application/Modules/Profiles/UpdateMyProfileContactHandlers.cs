using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Profiles;
using FluentValidation;
using FluentValidation.Results;
using Application.Shared;

namespace Application.Profiles;

public sealed record UpdateMyProfileContactCommand(
    string? ContactEmail,
    string? ContactPhone,
    string? Website,
    string? TimeZone) : ICommand<IResult>;

internal sealed class UpdateMyProfileContactCommandValidator : AbstractValidator<UpdateMyProfileContactCommand>
{
    public UpdateMyProfileContactCommandValidator()
    {
        RuleFor(x => x.ContactEmail).MaximumLength(320).When(x => !string.IsNullOrWhiteSpace(x.ContactEmail));
        RuleFor(x => x.ContactPhone).MaximumLength(32).When(x => !string.IsNullOrWhiteSpace(x.ContactPhone));
        RuleFor(x => x.Website).MaximumLength(400).When(x => !string.IsNullOrWhiteSpace(x.Website));
        RuleFor(x => x.TimeZone).MaximumLength(80).When(x => !string.IsNullOrWhiteSpace(x.TimeZone));
    }
}

internal sealed class UpdateMyProfileContactCommandHandler(
    IUserContext userContext,
    IApplicationDbContext writeContext,
    IValidator<UpdateMyProfileContactCommand> validator) : ResultWrappingCommandHandler<UpdateMyProfileContactCommand>
{
    protected override async Task<IResult> HandleCore(UpdateMyProfileContactCommand command, CancellationToken cancellationToken) =>
        await UpdateAsync(command, userContext, writeContext, validator, cancellationToken);

    private static async Task<IResult> UpdateAsync(
        UpdateMyProfileContactCommand request,
        IUserContext userContext,
        IApplicationDbContext writeContext,
        IValidator<UpdateMyProfileContactCommand> validator,
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

        profile.ContactEmail = InputSanitizer.SanitizeEmail(request.ContactEmail);
        profile.ContactPhone = InputSanitizer.SanitizeIdentifier(request.ContactPhone, 32);
        profile.WebsiteUrl = InputSanitizer.SanitizeText(request.Website, 400);
        profile.TimeZone = InputSanitizer.SanitizeIdentifier(request.TimeZone, 80);
        profile.LastProfileUpdateAtUtc = DateTime.UtcNow;
        profile.ProfileCompletenessScore = ProfileEndpointCommon.ComputeCompleteness(profile);
        profile.Raise(new UserProfileChangedDomainEvent(profile.Id, profile.UserId, "Contact", profile.ProfileCompletenessScore));

        await writeContext.SaveChangesAsync(cancellationToken);
        return Results.Ok(ProfileEndpointCommon.ToPrivateResponse(profile));
    }
}







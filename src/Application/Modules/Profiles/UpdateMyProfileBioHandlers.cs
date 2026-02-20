using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Profiles;
using FluentValidation;
using FluentValidation.Results;
using Application.Shared;

namespace Application.Profiles;

public sealed record UpdateMyProfileBioCommand(string? Bio) : ICommand<IResult>;

internal sealed class UpdateMyProfileBioCommandValidator : AbstractValidator<UpdateMyProfileBioCommand>
{
    public UpdateMyProfileBioCommandValidator() => RuleFor(x => x.Bio).MaximumLength(1200).When(x => !string.IsNullOrWhiteSpace(x.Bio));
}

internal sealed class UpdateMyProfileBioCommandHandler(
    IUserContext userContext,
    IApplicationDbContext writeContext,
    IValidator<UpdateMyProfileBioCommand> validator) : ResultWrappingCommandHandler<UpdateMyProfileBioCommand>
{
    protected override async Task<IResult> HandleCore(UpdateMyProfileBioCommand command, CancellationToken cancellationToken) =>
        await UpdateAsync(command, userContext, writeContext, validator, cancellationToken);

    private static async Task<IResult> UpdateAsync(
        UpdateMyProfileBioCommand request,
        IUserContext userContext,
        IApplicationDbContext writeContext,
        IValidator<UpdateMyProfileBioCommand> validator,
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

        profile.Bio = InputSanitizer.SanitizeText(request.Bio, 1200);
        profile.LastProfileUpdateAtUtc = DateTime.UtcNow;
        profile.ProfileCompletenessScore = ProfileEndpointCommon.ComputeCompleteness(profile);
        profile.Raise(new UserProfileChangedDomainEvent(profile.Id, profile.UserId, "Bio", profile.ProfileCompletenessScore));

        await writeContext.SaveChangesAsync(cancellationToken);
        return Results.Ok(ProfileEndpointCommon.ToPrivateResponse(profile));
    }
}







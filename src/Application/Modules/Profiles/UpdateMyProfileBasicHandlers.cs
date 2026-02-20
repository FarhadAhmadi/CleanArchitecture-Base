using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Profiles;
using FluentValidation;
using FluentValidation.Results;
using Application.Shared;

namespace Application.Profiles;

public sealed record UpdateMyProfileBasicCommand(
    string? DisplayName,
    string? Bio,
    DateTime? DateOfBirth,
    string? Gender,
    string? Location) : ICommand<IResult>;

internal sealed class UpdateMyProfileBasicCommandValidator : AbstractValidator<UpdateMyProfileBasicCommand>
{
    public UpdateMyProfileBasicCommandValidator()
    {
        RuleFor(x => x.DisplayName).MaximumLength(160).When(x => !string.IsNullOrWhiteSpace(x.DisplayName));
        RuleFor(x => x.Bio).MaximumLength(1200).When(x => !string.IsNullOrWhiteSpace(x.Bio));
        RuleFor(x => x.Gender).MaximumLength(32).When(x => !string.IsNullOrWhiteSpace(x.Gender));
        RuleFor(x => x.Location).MaximumLength(200).When(x => !string.IsNullOrWhiteSpace(x.Location));
        RuleFor(x => x.DateOfBirth).LessThanOrEqualTo(DateTime.UtcNow.Date).When(x => x.DateOfBirth.HasValue);
    }
}

internal sealed class UpdateMyProfileBasicCommandHandler(
    IUserContext userContext,
    IApplicationDbContext writeContext,
    IValidator<UpdateMyProfileBasicCommand> validator) : ResultWrappingCommandHandler<UpdateMyProfileBasicCommand>
{
    protected override async Task<IResult> HandleCore(UpdateMyProfileBasicCommand command, CancellationToken cancellationToken) =>
        await UpdateAsync(command, userContext, writeContext, validator, cancellationToken);

    private static async Task<IResult> UpdateAsync(
        UpdateMyProfileBasicCommand request,
        IUserContext userContext,
        IApplicationDbContext writeContext,
        IValidator<UpdateMyProfileBasicCommand> validator,
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







using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Profiles;
using FluentValidation;
using FluentValidation.Results;
using Application.Shared;

namespace Application.Profiles;

public sealed record UpdateMyProfileContactCommand(UpdateProfileContactRequest Request) : ICommand<IResult>;
internal sealed class UpdateMyProfileContactCommandHandler(
    IUserContext userContext,
    IApplicationDbContext writeContext,
    IValidator<UpdateProfileContactRequest> validator) : ResultWrappingCommandHandler<UpdateMyProfileContactCommand>
{
    protected override async Task<IResult> HandleCore(UpdateMyProfileContactCommand command, CancellationToken cancellationToken) =>
        await UpdateAsync(command.Request, userContext, writeContext, validator, cancellationToken);

    private static async Task<IResult> UpdateAsync(
        UpdateProfileContactRequest request,
        IUserContext userContext,
        IApplicationDbContext writeContext,
        IValidator<UpdateProfileContactRequest> validator,
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







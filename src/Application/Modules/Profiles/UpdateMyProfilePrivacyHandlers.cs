using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Profiles;
using FluentValidation;
using FluentValidation.Results;

namespace Application.Profiles;

public sealed record UpdateMyProfilePrivacyCommand(UpdateProfilePrivacyRequest Request) : ICommand<IResult>;
internal sealed class UpdateMyProfilePrivacyCommandHandler(
    IUserContext userContext,
    IApplicationDbContext writeContext,
    IValidator<UpdateProfilePrivacyRequest> validator) : ResultWrappingCommandHandler<UpdateMyProfilePrivacyCommand>
{
    protected override async Task<IResult> HandleCore(UpdateMyProfilePrivacyCommand command, CancellationToken cancellationToken) =>
        await UpdateAsync(command.Request, userContext, writeContext, validator, cancellationToken);

    private static async Task<IResult> UpdateAsync(
        UpdateProfilePrivacyRequest request,
        IUserContext userContext,
        IApplicationDbContext writeContext,
        IValidator<UpdateProfilePrivacyRequest> validator,
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

        profile.IsProfilePublic = request.IsProfilePublic;
        profile.ShowEmail = request.ShowEmail;
        profile.ShowPhone = request.ShowPhone;
        profile.LastProfileUpdateAtUtc = DateTime.UtcNow;
        profile.ProfileCompletenessScore = ProfileEndpointCommon.ComputeCompleteness(profile);
        profile.Raise(new UserProfileChangedDomainEvent(profile.Id, profile.UserId, "Privacy", profile.ProfileCompletenessScore));

        await writeContext.SaveChangesAsync(cancellationToken);
        return Results.Ok(ProfileEndpointCommon.ToPrivateResponse(profile));
    }
}







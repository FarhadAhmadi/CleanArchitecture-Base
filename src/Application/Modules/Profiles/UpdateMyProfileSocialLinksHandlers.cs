using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Profiles;
using FluentValidation;
using FluentValidation.Results;

namespace Application.Profiles;

public sealed record UpdateMyProfileSocialLinksCommand(Dictionary<string, string>? Links) : ICommand<IResult>;

internal sealed class UpdateMyProfileSocialLinksCommandValidator : AbstractValidator<UpdateMyProfileSocialLinksCommand>
{
    public UpdateMyProfileSocialLinksCommandValidator()
    {
        RuleFor(x => x.Links).Must(x => x is null || x.Count <= 20);
        RuleForEach(x => x.Links!).ChildRules(link =>
        {
            link.RuleFor(x => x.Key).NotEmpty().MaximumLength(50);
            link.RuleFor(x => x.Value).NotEmpty().MaximumLength(800);
        });
    }
}

internal sealed class UpdateMyProfileSocialLinksCommandHandler(
    IUserContext userContext,
    IApplicationDbContext writeContext,
    IValidator<UpdateMyProfileSocialLinksCommand> validator) : ResultWrappingCommandHandler<UpdateMyProfileSocialLinksCommand>
{
    protected override async Task<IResult> HandleCore(UpdateMyProfileSocialLinksCommand command, CancellationToken cancellationToken) =>
        await UpdateAsync(command, userContext, writeContext, validator, cancellationToken);

    private static async Task<IResult> UpdateAsync(
        UpdateMyProfileSocialLinksCommand request,
        IUserContext userContext,
        IApplicationDbContext writeContext,
        IValidator<UpdateMyProfileSocialLinksCommand> validator,
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

        profile.SocialLinksJson = ProfileEndpointCommon.BuildSocialLinksJson(request.Links);
        profile.LastProfileUpdateAtUtc = DateTime.UtcNow;
        profile.ProfileCompletenessScore = ProfileEndpointCommon.ComputeCompleteness(profile);
        profile.Raise(new UserProfileChangedDomainEvent(profile.Id, profile.UserId, "SocialLinks", profile.ProfileCompletenessScore));

        await writeContext.SaveChangesAsync(cancellationToken);
        return Results.Ok(ProfileEndpointCommon.ToPrivateResponse(profile));
    }
}







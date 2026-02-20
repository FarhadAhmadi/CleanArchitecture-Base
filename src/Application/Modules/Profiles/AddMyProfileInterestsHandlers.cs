using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Profiles;
using FluentValidation;
using FluentValidation.Results;
using Application.Shared;

namespace Application.Profiles;

public sealed record AddMyProfileInterestsCommand(List<string>? Interests) : ICommand<IResult>;

internal sealed class AddMyProfileInterestsCommandValidator : AbstractValidator<AddMyProfileInterestsCommand>
{
    public AddMyProfileInterestsCommandValidator()
    {
        RuleFor(x => x.Interests).NotNull().Must(x => x!.Count is > 0 and <= 20);
        RuleForEach(x => x.Interests!).NotEmpty().MaximumLength(60);
    }
}

internal sealed class AddMyProfileInterestsCommandHandler(
    IUserContext userContext,
    IApplicationDbContext writeContext,
    IValidator<AddMyProfileInterestsCommand> validator) : ResultWrappingCommandHandler<AddMyProfileInterestsCommand>
{
    protected override async Task<IResult> HandleCore(AddMyProfileInterestsCommand command, CancellationToken cancellationToken) =>
        await AddAsync(command, userContext, writeContext, validator, cancellationToken);

    private static async Task<IResult> AddAsync(
        AddMyProfileInterestsCommand request,
        IUserContext userContext,
        IApplicationDbContext writeContext,
        IValidator<AddMyProfileInterestsCommand> validator,
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

        HashSet<string> currentInterests = [.. ProfileEndpointCommon.ParseInterests(profile.InterestsCsv)];
        foreach (string interest in InputSanitizer.SanitizeList(request.Interests ?? [], 60))
        {
            currentInterests.Add(interest);
        }

        profile.InterestsCsv = string.Join(',', currentInterests.OrderBy(x => x, StringComparer.OrdinalIgnoreCase));
        profile.LastProfileUpdateAtUtc = DateTime.UtcNow;
        profile.ProfileCompletenessScore = ProfileEndpointCommon.ComputeCompleteness(profile);
        profile.Raise(new UserProfileChangedDomainEvent(profile.Id, profile.UserId, "Interests", profile.ProfileCompletenessScore));

        await writeContext.SaveChangesAsync(cancellationToken);
        return Results.Ok(ProfileEndpointCommon.ToPrivateResponse(profile));
    }
}







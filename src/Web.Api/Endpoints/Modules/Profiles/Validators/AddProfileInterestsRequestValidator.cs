using FluentValidation;

namespace Web.Api.Endpoints.Profiles;

internal sealed class AddProfileInterestsRequestValidator : AbstractValidator<AddProfileInterestsRequest>
{
    public AddProfileInterestsRequestValidator()
    {
        RuleFor(x => x.Interests)
            .NotNull()
            .Must(x => x is { Count: > 0 and <= 50 })
            .WithMessage("At least one and at most 50 interests are required.");

        RuleForEach(x => x.Interests!)
            .NotEmpty()
            .MaximumLength(60);
    }
}

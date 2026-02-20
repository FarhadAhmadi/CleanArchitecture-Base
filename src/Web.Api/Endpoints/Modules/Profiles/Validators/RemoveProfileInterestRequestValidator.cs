using FluentValidation;

namespace Web.Api.Endpoints.Profiles;

internal sealed class RemoveProfileInterestRequestValidator : AbstractValidator<RemoveProfileInterestRequest>
{
    public RemoveProfileInterestRequestValidator()
    {
        RuleFor(x => x.Interest)
            .NotEmpty()
            .MaximumLength(60);
    }
}

using FluentValidation;

namespace Web.Api.Endpoints.Profiles;

internal sealed class CreateProfileRequestValidator : AbstractValidator<CreateProfileRequest>
{
    public CreateProfileRequestValidator()
    {
        RuleFor(x => x.DisplayName)
            .NotEmpty()
            .MaximumLength(160);

        RuleFor(x => x.PreferredLanguage)
            .MaximumLength(16)
            .When(x => !string.IsNullOrWhiteSpace(x.PreferredLanguage));
    }
}

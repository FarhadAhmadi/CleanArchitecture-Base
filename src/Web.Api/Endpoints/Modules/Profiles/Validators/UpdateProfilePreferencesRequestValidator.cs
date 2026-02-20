using FluentValidation;

namespace Web.Api.Endpoints.Profiles;

internal sealed class UpdateProfilePreferencesRequestValidator : AbstractValidator<UpdateProfilePreferencesRequest>
{
    public UpdateProfilePreferencesRequestValidator()
    {
        RuleFor(x => x.PreferredLanguage)
            .NotEmpty()
            .MaximumLength(16);
    }
}

using FluentValidation;

namespace Web.Api.Endpoints.Profiles;

internal sealed class UpdateProfilePrivacyRequestValidator : AbstractValidator<UpdateProfilePrivacyRequest>
{
    public UpdateProfilePrivacyRequestValidator()
    {
        RuleFor(x => x).NotNull();
    }
}

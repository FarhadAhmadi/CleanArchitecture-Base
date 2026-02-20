using FluentValidation;

namespace Web.Api.Endpoints.Profiles;

internal sealed class UpdateProfileBioRequestValidator : AbstractValidator<UpdateProfileBioRequest>
{
    public UpdateProfileBioRequestValidator()
    {
        RuleFor(x => x.Bio)
            .NotEmpty()
            .MaximumLength(1200);
    }
}

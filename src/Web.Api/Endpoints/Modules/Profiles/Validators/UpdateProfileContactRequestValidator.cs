using FluentValidation;

namespace Web.Api.Endpoints.Profiles;

internal sealed class UpdateProfileContactRequestValidator : AbstractValidator<UpdateProfileContactRequest>
{
    public UpdateProfileContactRequestValidator()
    {
        RuleFor(x => x.ContactEmail)
            .EmailAddress()
            .MaximumLength(320)
            .When(x => !string.IsNullOrWhiteSpace(x.ContactEmail));

        RuleFor(x => x.ContactPhone)
            .MaximumLength(32)
            .When(x => !string.IsNullOrWhiteSpace(x.ContactPhone));

        RuleFor(x => x.Website)
            .MaximumLength(400)
            .When(x => !string.IsNullOrWhiteSpace(x.Website));

        RuleFor(x => x.TimeZone)
            .MaximumLength(80)
            .When(x => !string.IsNullOrWhiteSpace(x.TimeZone));
    }
}

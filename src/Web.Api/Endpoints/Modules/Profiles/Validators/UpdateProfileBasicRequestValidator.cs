using FluentValidation;

namespace Web.Api.Endpoints.Profiles;

internal sealed class UpdateProfileBasicRequestValidator : AbstractValidator<UpdateProfileBasicRequest>
{
    public UpdateProfileBasicRequestValidator()
    {
        RuleFor(x => x.DisplayName)
            .MaximumLength(160)
            .When(x => !string.IsNullOrWhiteSpace(x.DisplayName));

        RuleFor(x => x.Bio)
            .MaximumLength(1200)
            .When(x => !string.IsNullOrWhiteSpace(x.Bio));

        RuleFor(x => x.Gender)
            .MaximumLength(32)
            .When(x => !string.IsNullOrWhiteSpace(x.Gender));

        RuleFor(x => x.Location)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.Location));

        RuleFor(x => x.DateOfBirth)
            .LessThanOrEqualTo(DateTime.UtcNow.Date)
            .When(x => x.DateOfBirth.HasValue);
    }
}

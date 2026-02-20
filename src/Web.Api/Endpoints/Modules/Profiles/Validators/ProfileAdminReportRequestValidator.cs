using FluentValidation;

namespace Web.Api.Endpoints.Profiles;

internal sealed class ProfileAdminReportRequestValidator : AbstractValidator<ProfileAdminReportRequest>
{
    public ProfileAdminReportRequestValidator()
    {
        RuleFor(x => x.Search)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.Search));

        RuleFor(x => x.PreferredLanguage)
            .MaximumLength(16)
            .When(x => !string.IsNullOrWhiteSpace(x.PreferredLanguage));

        RuleFor(x => x.MinCompleteness)
            .InclusiveBetween(0, 100)
            .When(x => x.MinCompleteness.HasValue);

        RuleFor(x => x.MaxCompleteness)
            .InclusiveBetween(0, 100)
            .When(x => x.MaxCompleteness.HasValue);

        RuleFor(x => x)
            .Must(x => !x.MinCompleteness.HasValue || !x.MaxCompleteness.HasValue || x.MinCompleteness <= x.MaxCompleteness)
            .WithMessage("minCompletion cannot be greater than maxCompletion.");

        RuleFor(x => x)
            .Must(x => !x.UpdatedFrom.HasValue || !x.UpdatedTo.HasValue || x.UpdatedFrom <= x.UpdatedTo)
            .WithMessage("updatedFrom cannot be greater than updatedTo.");
    }
}

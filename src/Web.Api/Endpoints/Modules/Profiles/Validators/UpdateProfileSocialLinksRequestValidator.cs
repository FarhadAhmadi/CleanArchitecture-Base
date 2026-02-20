using FluentValidation;

namespace Web.Api.Endpoints.Profiles;

internal sealed class UpdateProfileSocialLinksRequestValidator : AbstractValidator<UpdateProfileSocialLinksRequest>
{
    public UpdateProfileSocialLinksRequestValidator()
    {
        RuleFor(x => x.Links)
            .Must(links => links is null || links.Count <= 20)
            .WithMessage("A maximum of 20 social links is allowed.");

        RuleForEach(x => x.Links!)
            .ChildRules(link =>
            {
                link.RuleFor(x => x.Key).NotEmpty().MaximumLength(50);
                link.RuleFor(x => x.Value).NotEmpty().MaximumLength(800);
            })
            .When(x => x.Links is not null);
    }
}

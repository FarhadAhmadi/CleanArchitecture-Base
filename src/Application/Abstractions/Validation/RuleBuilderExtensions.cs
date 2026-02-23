using FluentValidation;

namespace Application.Abstractions.Validation;

public static class RuleBuilderExtensions
{
    public static IRuleBuilderOptions<T, Guid> NotEmptyGuid<T>(this IRuleBuilder<T, Guid> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithMessage("'{PropertyName}' must be a non-empty GUID.");
    }

    public static IRuleBuilderOptions<T, string> StrongPassword<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .MinimumLength(12)
            .MaximumLength(200)
            .Matches("[A-Z]").WithMessage("'{PropertyName}' must contain at least one uppercase letter.")
            .Matches("[a-z]").WithMessage("'{PropertyName}' must contain at least one lowercase letter.")
            .Matches("[0-9]").WithMessage("'{PropertyName}' must contain at least one number.")
            .Matches(@"[^a-zA-Z0-9]").WithMessage("'{PropertyName}' must contain at least one special character.");
    }
}

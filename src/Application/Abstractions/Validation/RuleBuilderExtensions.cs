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
}

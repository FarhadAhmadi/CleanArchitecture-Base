using FluentValidation.Results;

namespace Application.Profiles;

internal static class ProfileValidationExtensions
{
    internal static IResult ToValidationProblem(this ValidationResult validationResult)
    {
        var errors = validationResult.Errors
            .GroupBy(x => x.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(x => x.ErrorMessage).Distinct(StringComparer.Ordinal).ToArray(),
                StringComparer.Ordinal);

        return Results.ValidationProblem(errors);
    }
}



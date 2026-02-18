using Application.Abstractions.Validation;
using FluentValidation;

namespace Application.Todos.Create;

public sealed class CreateTodoCommandValidator : AbstractValidator<CreateTodoCommand>
{
    public CreateTodoCommandValidator()
    {
        RuleFor(c => c.UserId).NotEmptyGuid();
        RuleFor(c => c.Priority).IsInEnum();
        RuleFor(c => c.Description).NotEmpty().MaximumLength(255);
        RuleFor(c => c.DueDate).GreaterThanOrEqualTo(DateTime.Today).When(x => x.DueDate.HasValue);
    }
}

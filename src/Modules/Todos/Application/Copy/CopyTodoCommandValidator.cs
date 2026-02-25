using Application.Abstractions.Validation;
using FluentValidation;

namespace Application.Todos.Copy;

public sealed class CopyTodoCommandValidator : AbstractValidator<CopyTodoCommand>
{
    public CopyTodoCommandValidator()
    {
        RuleFor(c => c.UserId).NotEmptyGuid();
        RuleFor(c => c.TodoId).NotEmptyGuid();
    }
}

using Application.Abstractions.Validation;
using FluentValidation;

namespace Application.Todos.Delete;

internal sealed class DeleteTodoCommandValidator : AbstractValidator<DeleteTodoCommand>
{
    public DeleteTodoCommandValidator()
    {
        RuleFor(c => c.TodoItemId).NotEmptyGuid();
    }
}

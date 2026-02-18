using Domain.Todos;

namespace Application.Todos.Create;

internal static class CreateTodoMappings
{
    internal static TodoItem ToEntity(this CreateTodoCommand command, DateTime createdAt)
    {
        return new TodoItem
        {
            UserId = command.UserId,
            Description = command.Description,
            Priority = command.Priority,
            DueDate = command.DueDate,
            Labels = command.Labels,
            IsCompleted = false,
            CreatedAt = createdAt
        };
    }
}

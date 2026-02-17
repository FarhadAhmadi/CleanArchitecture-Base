using Domain.Todos;

namespace Application.Todos.Copy;

internal static class CopyTodoMappings
{
    internal static TodoItem ToEntity(this CopyTodoCommand command, TodoItem sourceTodo, DateTime createdAt)
    {
        return new TodoItem
        {
            UserId = command.UserId,
            Description = sourceTodo.Description,
            Priority = sourceTodo.Priority,
            DueDate = sourceTodo.DueDate,
            Labels = sourceTodo.Labels.ToList(),
            IsCompleted = false,
            CreatedAt = createdAt
        };
    }
}

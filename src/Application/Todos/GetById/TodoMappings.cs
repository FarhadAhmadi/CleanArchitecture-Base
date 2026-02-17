using System.Linq.Expressions;
using Domain.Todos;

namespace Application.Todos.GetById;

internal static class TodoMappings
{
    internal static readonly Expression<Func<TodoItem, TodoResponse>> ToModel = todoItem => new TodoResponse
    {
        Id = todoItem.Id,
        UserId = todoItem.UserId,
        Description = todoItem.Description,
        DueDate = todoItem.DueDate,
        Labels = todoItem.Labels,
        IsCompleted = todoItem.IsCompleted,
        CreatedAt = todoItem.CreatedAt,
        CompletedAt = todoItem.CompletedAt
    };
}

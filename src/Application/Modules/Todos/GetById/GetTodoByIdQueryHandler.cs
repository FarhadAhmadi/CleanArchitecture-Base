using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Todos;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Todos.GetById;

internal sealed class GetTodoByIdQueryHandler(IApplicationReadDbContext context, IUserContext userContext)
    : IQueryHandler<GetTodoByIdQuery, TodoResponse>
{
    public async Task<Result<TodoResponse>> Handle(GetTodoByIdQuery query, CancellationToken cancellationToken)
    {
        TodoResponse? todo = await context.TodoItems
            .Where(todoItem => todoItem.Id == query.TodoItemId && todoItem.UserId == userContext.UserId)
            .Select(TodoMappings.ToModel)
            .SingleOrDefaultAsync(cancellationToken);

        if (todo is null)
        {
            return Result.Failure<TodoResponse>(TodoItemErrors.NotFound(query.TodoItemId));
        }

        return todo;
    }
}

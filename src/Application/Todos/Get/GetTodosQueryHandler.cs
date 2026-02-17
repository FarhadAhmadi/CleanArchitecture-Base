using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Todos;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Todos.Get;

internal sealed class GetTodosQueryHandler(IApplicationReadDbContext context, IUserContext userContext)
    : IQueryHandler<GetTodosQuery, PagedResponse<TodoResponse>>
{
    public async Task<Result<PagedResponse<TodoResponse>>> Handle(GetTodosQuery query, CancellationToken cancellationToken)
    {
        if (query.UserId != userContext.UserId)
        {
            return Result.Failure<PagedResponse<TodoResponse>>(UserErrors.Unauthorized());
        }

        IQueryable<TodoItem> todoQuery = context.TodoItems.Where(todoItem => todoItem.UserId == query.UserId);

        todoQuery = todoQuery.ApplyContainsSearch(query.Search, x => x.Description);

        if (query.IsCompleted.HasValue)
        {
            todoQuery = todoQuery.Where(x => x.IsCompleted == query.IsCompleted.Value);
        }

        todoQuery = ApplySorting(todoQuery, query.SortBy, query.SortOrder);

        (int page, int pageSize) = QueryableExtensions.NormalizePaging(
            query.Page,
            query.PageSize,
            defaultPageSize: 20,
            maxPageSize: 100);

        int totalCount = await todoQuery.CountAsync(cancellationToken);

        List<TodoResponse> items = await todoQuery
            .ApplyPaging(page, pageSize)
            .Select(TodoMappings.ToModel)
            .ToListAsync(cancellationToken);

        return new PagedResponse<TodoResponse>(items, page, pageSize, totalCount);
    }

    private static IQueryable<TodoItem> ApplySorting(
        IQueryable<TodoItem> query,
        string? sortBy,
        string? sortOrder)
    {
        bool isDesc = string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase);

        return sortBy?.ToUpperInvariant() switch
        {
            "DUEDATE" => isDesc ? query.OrderByDescending(x => x.DueDate) : query.OrderBy(x => x.DueDate),
            "PRIORITY" => isDesc ? query.OrderByDescending(x => x.Priority) : query.OrderBy(x => x.Priority),
            _ => isDesc ? query.OrderByDescending(x => x.CreatedAt) : query.OrderBy(x => x.CreatedAt)
        };
    }
}

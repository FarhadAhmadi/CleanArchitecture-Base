using Application.Abstractions.Messaging;

namespace Application.Todos.Get;

public sealed record GetTodosQuery(
    Guid UserId,
    int Page,
    int PageSize,
    string? Search,
    bool? IsCompleted,
    string? SortBy,
    string? SortOrder) : IQuery<PagedResponse<TodoResponse>>;

using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Application.Todos.Get;
using SharedKernel;
using Web.Api.Endpoints.Mappings;
using Web.Api.Endpoints.Users;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Todos;

internal sealed class Get : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("todos", async (
            IUserContext userContext,
            int page,
            int pageSize,
            string? search,
            bool? isCompleted,
            string? sortBy,
            string? sortOrder,
            IQueryHandler<GetTodosQuery, PagedResponse<TodoResponse>> handler,
            CancellationToken cancellationToken) =>
        {
            page = page <= 0 ? 1 : page;
            pageSize = pageSize is <= 0 or > 100 ? 20 : pageSize;

            var query = userContext.UserId.ToGetTodosQuery(
                page,
                pageSize,
                search,
                isCompleted,
                sortBy,
                sortOrder);

            Result<PagedResponse<TodoResponse>> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Todos)
        .HasPermission(Permissions.TodosRead);
    }
}

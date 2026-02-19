using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Application.Todos.Get;
using Microsoft.AspNetCore.Mvc;
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
            IQueryHandler<GetTodosQuery, PagedResponse<TodoResponse>> handler,
            CancellationToken cancellationToken,
            [AsParameters] GetTodosRequest request) =>
        {
            (int page, int pageSize) = request.NormalizePaging();

            var query = userContext.UserId.ToGetTodosQuery(
                page,
                pageSize,
                request);

            Result<PagedResponse<TodoResponse>> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Todos)
        .HasPermission(Permissions.TodosRead);
    }
}

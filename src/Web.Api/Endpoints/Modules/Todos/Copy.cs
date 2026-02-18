using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Application.Todos.Copy;
using SharedKernel;
using Web.Api.Endpoints.Mappings;
using Web.Api.Endpoints.Users;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Todos;

internal sealed class Copy : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("todos/{todoId:guid}/copy", async (
            Guid todoId,
            IUserContext userContext,
            ICommandHandler<CopyTodoCommand, Guid> handler,
            CancellationToken cancellationToken) =>
        {
            var command = todoId.ToCopyTodoCommand(userContext.UserId);

            Result<Guid> result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Todos)
        .HasPermission(Permissions.TodosWrite);
    }
}

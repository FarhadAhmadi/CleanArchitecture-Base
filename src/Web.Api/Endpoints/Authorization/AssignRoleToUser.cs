using Application.Abstractions.Messaging;
using Application.Authorization.AssignRoleToUser;
using SharedKernel;
using Web.Api.Endpoints.Users;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Authorization;

internal sealed class AssignRoleToUser : IEndpoint
{
    public sealed record Request(Guid UserId, string RoleName);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("authorization/assign-role", async (
            Request request,
            ICommandHandler<AssignRoleToUserCommand> handler,
            CancellationToken cancellationToken) =>
        {
            AssignRoleToUserCommand command = new(request.UserId, request.RoleName);
            Result result = await handler.Handle(command, cancellationToken);
            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .WithTags(Tags.Users)
        .HasPermission(Permissions.AuthorizationManage);
    }
}

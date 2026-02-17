using Application.Abstractions.Messaging;
using Application.Authorization.AssignPermissionToRole;
using SharedKernel;
using Web.Api.Endpoints.Users;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Authorization;

internal sealed class AssignPermissionToRole : IEndpoint
{
    public sealed record Request(string RoleName, string PermissionCode);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("authorization/assign-permission", async (
            Request request,
            ICommandHandler<AssignPermissionToRoleCommand> handler,
            CancellationToken cancellationToken) =>
        {
            AssignPermissionToRoleCommand command = new(request.RoleName, request.PermissionCode);
            Result result = await handler.Handle(command, cancellationToken);
            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .WithTags(Tags.Users)
        .HasPermission(Permissions.AuthorizationManage);
    }
}

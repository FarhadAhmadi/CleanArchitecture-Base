using Application.Abstractions.Messaging;
using Web.Api.Extensions;

namespace Web.Api.Endpoints.Logging;

internal static class MapLoggingCreateRoleEndpointRoute
{
    internal static void MapLoggingCreateRole(this RouteGroupBuilder group)
    {
        group.MapPost("/access-control/roles", async (
            CreateRoleRequest request,
            ICommandHandler<CreateLoggingRoleCommand, IResult> handler,
            CancellationToken cancellationToken) =>
            await handler.Handle(new CreateLoggingRoleCommand(request), cancellationToken))
            .HasPermission(LoggingPermissions.AccessManage);
    }
}



using Application.Abstractions.Messaging;
using Web.Api.Extensions;

namespace Web.Api.Endpoints.Logging;

internal static class MapLoggingAssignAccessEndpointRoute
{
    internal static void MapLoggingAssignAccess(this RouteGroupBuilder group)
    {
        group.MapPost("/access-control/assign", async (
            AssignAccessRequest request,
            ICommandHandler<AssignLoggingAccessCommand, IResult> handler,
            CancellationToken cancellationToken) =>
            await handler.Handle(new AssignLoggingAccessCommand(request), cancellationToken))
            .HasPermission(LoggingPermissions.AccessManage);
    }
}



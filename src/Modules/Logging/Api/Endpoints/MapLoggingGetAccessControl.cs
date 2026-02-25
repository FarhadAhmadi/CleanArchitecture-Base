using Application.Abstractions.Messaging;
using Web.Api.Extensions;

namespace Web.Api.Endpoints.Logging;

internal static class MapLoggingGetAccessControlEndpointRoute
{
    internal static void MapLoggingGetAccessControl(this RouteGroupBuilder group)
    {
        group.MapGet("/access-control", async (
            IQueryHandler<GetLoggingAccessControlQuery, IResult> handler,
            CancellationToken cancellationToken) =>
            await handler.Handle(new GetLoggingAccessControlQuery(), cancellationToken))
            .HasPermission(LoggingPermissions.AccessManage);
    }
}



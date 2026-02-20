using Application.Abstractions.Messaging;
using Web.Api.Extensions;

namespace Web.Api.Endpoints.Logging;

internal static class MapLoggingHealthEndpointRoute
{
    internal static void MapLoggingHealth(this RouteGroupBuilder group)
    {
        group.MapGet("/health", async (
            IQueryHandler<GetLoggingHealthQuery, IResult> handler,
            CancellationToken cancellationToken) =>
            await handler.Handle(new GetLoggingHealthQuery(), cancellationToken))
            .HasPermission(LoggingPermissions.EventsRead);
    }
}



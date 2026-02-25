using Application.Abstractions.Messaging;
using Web.Api.Extensions;

namespace Web.Api.Endpoints.Logging;

internal static class MapLoggingGetCorruptedEventsEndpointRoute
{
    internal static void MapLoggingGetCorruptedEvents(this RouteGroupBuilder group)
    {
        group.MapGet("/events/corrupted", async (
            bool recalculate,
            IQueryHandler<GetCorruptedLogEventsQuery, IResult> handler,
            CancellationToken cancellationToken) =>
            await handler.Handle(new GetCorruptedLogEventsQuery(recalculate), cancellationToken))
            .HasPermission(LoggingPermissions.EventsRead);
    }
}



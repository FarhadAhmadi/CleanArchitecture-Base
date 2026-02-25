using Application.Abstractions.Messaging;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Extensions;

namespace Web.Api.Endpoints.Logging;

internal static class MapLoggingGetEventsEndpointRoute
{
    internal static void MapLoggingGetEvents(this RouteGroupBuilder group)
    {
        group.MapGet("/events", async (
            [AsParameters] GetLogEventsRequest request,
            IQueryHandler<GetLogEventsQuery, IResult> handler,
            CancellationToken cancellationToken) =>
            await handler.Handle(new GetLogEventsQuery(request), cancellationToken))
            .HasPermission(LoggingPermissions.EventsRead);
    }
}



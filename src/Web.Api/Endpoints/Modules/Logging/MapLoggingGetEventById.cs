using Application.Abstractions.Messaging;
using Web.Api.Extensions;

namespace Web.Api.Endpoints.Logging;

internal static class MapLoggingGetEventByIdEndpointRoute
{
    internal static void MapLoggingGetEventById(this RouteGroupBuilder group)
    {
        group.MapGet("/events/{eventId:guid}", async (
            Guid eventId,
            bool recalculateIntegrity,
            IQueryHandler<GetLogEventByIdQuery, IResult> handler,
            CancellationToken cancellationToken) =>
            await handler.Handle(new GetLogEventByIdQuery(eventId, recalculateIntegrity), cancellationToken))
            .HasPermission(LoggingPermissions.EventsRead);
    }
}



using Application.Abstractions.Messaging;
using Web.Api.Extensions;

namespace Web.Api.Endpoints.Logging;

internal static class MapLoggingDeleteEventEndpointRoute
{
    internal static void MapLoggingDeleteEvent(this RouteGroupBuilder group)
    {
        group.MapDelete("/events/{eventId:guid}", async (
            Guid eventId,
            ICommandHandler<DeleteLogEventCommand, IResult> handler,
            CancellationToken cancellationToken) =>
            await handler.Handle(new DeleteLogEventCommand(eventId), cancellationToken))
            .HasPermission(LoggingPermissions.EventsDelete);
    }
}



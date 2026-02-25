using Application.Abstractions.Messaging;
using Web.Api.Extensions;

namespace Web.Api.Endpoints.Logging;

internal static class MapLoggingIngestBulkEventsEndpointRoute
{
    internal static void MapLoggingIngestBulkEvents(this RouteGroupBuilder group)
    {
        group.MapPost("/events/bulk", async (
            BulkIngestRequest request,
            HttpContext httpContext,
            ICommandHandler<IngestBulkLogsCommand, IResult> handler,
            CancellationToken cancellationToken) =>
            await handler.Handle(
                new IngestBulkLogsCommand(request, httpContext.Request.Headers["Idempotency-Key"].FirstOrDefault()),
                cancellationToken))
            .HasPermission(LoggingPermissions.EventsWrite);
    }
}



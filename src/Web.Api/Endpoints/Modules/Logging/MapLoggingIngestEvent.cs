using Application.Abstractions.Messaging;
using Infrastructure.Logging;
using Web.Api.Extensions;

namespace Web.Api.Endpoints.Logging;

internal static class MapLoggingIngestEventEndpointRoute
{
    internal static void MapLoggingIngestEvent(this RouteGroupBuilder group)
    {
        group.MapPost("/events", async (
            IngestLogRequest request,
            HttpContext httpContext,
            ICommandHandler<IngestSingleLogCommand, IResult> handler,
            CancellationToken cancellationToken) =>
            await handler.Handle(
                new IngestSingleLogCommand(request, httpContext.Request.Headers["Idempotency-Key"].FirstOrDefault()),
                cancellationToken))
            .HasPermission(LoggingPermissions.EventsWrite);
    }
}



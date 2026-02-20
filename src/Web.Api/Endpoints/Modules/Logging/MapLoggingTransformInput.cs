using Application.Abstractions.Messaging;
using Infrastructure.Logging;
using Web.Api.Extensions;

namespace Web.Api.Endpoints.Logging;

internal static class MapLoggingTransformInputEndpointRoute
{
    internal static void MapLoggingTransformInput(this RouteGroupBuilder group)
    {
        group.MapPost("/transform", async (
            IngestLogRequest request,
            IQueryHandler<TransformLogInputQuery, IResult> handler,
            CancellationToken cancellationToken) =>
            await handler.Handle(new TransformLogInputQuery(request), cancellationToken))
            .HasPermission(LoggingPermissions.EventsWrite);
    }
}



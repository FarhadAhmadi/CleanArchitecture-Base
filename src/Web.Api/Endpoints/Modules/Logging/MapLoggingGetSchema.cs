using Application.Abstractions.Messaging;
using Web.Api.Extensions;

namespace Web.Api.Endpoints.Logging;

internal static class MapLoggingGetSchemaEndpointRoute
{
    internal static void MapLoggingGetSchema(this RouteGroupBuilder group)
    {
        group.MapGet("/schema", async (
            IQueryHandler<GetLogSchemaQuery, IResult> handler,
            CancellationToken cancellationToken) =>
            await handler.Handle(new GetLogSchemaQuery(), cancellationToken))
            .HasPermission(LoggingPermissions.EventsRead);
    }
}



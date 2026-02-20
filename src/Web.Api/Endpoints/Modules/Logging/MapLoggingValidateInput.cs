using Application.Abstractions.Messaging;
using Infrastructure.Logging;
using Web.Api.Extensions;

namespace Web.Api.Endpoints.Logging;

internal static class MapLoggingValidateInputEndpointRoute
{
    internal static void MapLoggingValidateInput(this RouteGroupBuilder group)
    {
        group.MapPost("/validate", async (
            IngestLogRequest request,
            IQueryHandler<ValidateLogInputQuery, IResult> handler,
            CancellationToken cancellationToken) =>
            await handler.Handle(new ValidateLogInputQuery(request), cancellationToken))
            .HasPermission(LoggingPermissions.EventsWrite);
    }
}



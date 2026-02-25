using Application.Abstractions.Messaging;
using Web.Api.Extensions;

namespace Web.Api.Endpoints.Logging;

internal static class MapLoggingGetAlertIncidentByIdEndpointRoute
{
    internal static void MapLoggingGetAlertIncidentById(this RouteGroupBuilder group)
    {
        group.MapGet("/alerts/incidents/{id:guid}", async (
            Guid id,
            IQueryHandler<GetAlertIncidentByIdQuery, IResult> handler,
            CancellationToken cancellationToken) =>
            await handler.Handle(new GetAlertIncidentByIdQuery(id), cancellationToken))
            .HasPermission(LoggingPermissions.AlertsManage);
    }
}



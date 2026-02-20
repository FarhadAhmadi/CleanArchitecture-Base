using Application.Abstractions.Messaging;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Extensions;

namespace Web.Api.Endpoints.Logging;

internal static class MapLoggingGetAlertIncidentsEndpointRoute
{
    internal static void MapLoggingGetAlertIncidents(this RouteGroupBuilder group)
    {
        group.MapGet("/alerts/incidents", async (
            [AsParameters] GetAlertIncidentsRequest request,
            IQueryHandler<GetAlertIncidentsQuery, IResult> handler,
            CancellationToken cancellationToken) =>
            await handler.Handle(new GetAlertIncidentsQuery(request.Page, request.PageSize, request.Status), cancellationToken))
            .HasPermission(LoggingPermissions.AlertsManage);
    }
}



using Application.Abstractions.Messaging;
using Web.Api.Extensions;

namespace Web.Api.Endpoints.Logging;

internal static class MapLoggingGetAlertRulesEndpointRoute
{
    internal static void MapLoggingGetAlertRules(this RouteGroupBuilder group)
    {
        group.MapGet("/alerts/rules", async (
            IQueryHandler<GetAlertRulesQuery, IResult> handler,
            CancellationToken cancellationToken) =>
            await handler.Handle(new GetAlertRulesQuery(), cancellationToken))
            .HasPermission(LoggingPermissions.AlertsManage);
    }
}



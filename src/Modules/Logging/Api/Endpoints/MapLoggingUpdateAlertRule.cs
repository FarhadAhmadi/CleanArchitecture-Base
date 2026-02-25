using Application.Abstractions.Messaging;
using Web.Api.Extensions;

namespace Web.Api.Endpoints.Logging;

internal static class MapLoggingUpdateAlertRuleEndpointRoute
{
    internal static void MapLoggingUpdateAlertRule(this RouteGroupBuilder group)
    {
        group.MapPut("/alerts/rules/{id:guid}", async (
            Guid id,
            UpdateAlertRuleRequest request,
            ICommandHandler<UpdateAlertRuleCommand, IResult> handler,
            CancellationToken cancellationToken) =>
            await handler.Handle(new UpdateAlertRuleCommand(id, request), cancellationToken))
            .HasPermission(LoggingPermissions.AlertsManage);
    }
}



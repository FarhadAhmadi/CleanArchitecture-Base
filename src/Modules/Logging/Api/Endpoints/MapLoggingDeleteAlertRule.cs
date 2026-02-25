using Application.Abstractions.Messaging;
using Web.Api.Extensions;

namespace Web.Api.Endpoints.Logging;

internal static class MapLoggingDeleteAlertRuleEndpointRoute
{
    internal static void MapLoggingDeleteAlertRule(this RouteGroupBuilder group)
    {
        group.MapDelete("/alerts/rules/{id:guid}", async (
            Guid id,
            ICommandHandler<DeleteAlertRuleCommand, IResult> handler,
            CancellationToken cancellationToken) =>
            await handler.Handle(new DeleteAlertRuleCommand(id), cancellationToken))
            .HasPermission(LoggingPermissions.AlertsManage);
    }
}



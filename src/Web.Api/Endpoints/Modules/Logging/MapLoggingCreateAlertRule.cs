using Application.Abstractions.Messaging;
using Web.Api.Extensions;

namespace Web.Api.Endpoints.Logging;

internal static class MapLoggingCreateAlertRuleEndpointRoute
{
    internal static void MapLoggingCreateAlertRule(this RouteGroupBuilder group)
    {
        group.MapPost("/alerts/rules", async (
            CreateAlertRuleRequest request,
            ICommandHandler<CreateAlertRuleCommand, IResult> handler,
            CancellationToken cancellationToken) =>
            await handler.Handle(new CreateAlertRuleCommand(request), cancellationToken))
            .HasPermission(LoggingPermissions.AlertsManage);
    }
}



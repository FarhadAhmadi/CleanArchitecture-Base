using Domain.Authorization;

namespace Web.Api.Endpoints.Logging;

internal static class LoggingPermissions
{
    internal const string EventsWrite = PermissionCodes.LoggingEventsWrite;
    internal const string EventsRead = PermissionCodes.LoggingEventsRead;
    internal const string EventsDelete = PermissionCodes.LoggingEventsDelete;
    internal const string AlertsManage = PermissionCodes.LoggingAlertsManage;
    internal const string AccessManage = PermissionCodes.LoggingAccessManage;
}

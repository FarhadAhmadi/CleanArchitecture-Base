namespace Domain.Authorization;

public static class PermissionCodes
{
    public const string UsersAccess = "users:access";
    public const string TodosRead = "todos:read";
    public const string TodosWrite = "todos:write";
    public const string AuthorizationManage = "authorization:manage";
    public const string LoggingEventsWrite = "logging.events.write";
    public const string LoggingEventsRead = "logging.events.read";
    public const string LoggingEventsDelete = "logging.events.delete";
    public const string LoggingAlertsManage = "logging.alerts.manage";
    public const string LoggingAccessManage = "logging.access.manage";
    public const string LoggingExportRead = "logging.export.read";
}

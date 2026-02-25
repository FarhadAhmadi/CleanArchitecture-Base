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
    public const string ObservabilityRead = "observability.read";
    public const string ObservabilityManage = "observability.manage";
    public const string AuditRead = "audit.read";
    public const string AuditManage = "audit.manage";
    public const string FilesRead = "files.read";
    public const string FilesWrite = "files.write";
    public const string FilesDelete = "files.delete";
    public const string FilesShare = "files.share";
    public const string FilesPermissionsManage = "files.permissions.manage";
    public const string NotificationsRead = "notifications.read";
    public const string NotificationsWrite = "notifications.write";
    public const string NotificationTemplatesManage = "notifications.templates.manage";
    public const string NotificationSchedulesManage = "notifications.schedules.manage";
    public const string NotificationPermissionsManage = "notifications.permissions.manage";
    public const string NotificationReportsRead = "notifications.reports.read";
    public const string ProfilesRead = "profiles.read";
    public const string ProfilesWrite = "profiles.write";
    public const string ProfilesPublicRead = "profiles.public.read";
    public const string ProfilesAdminRead = "profiles.admin.read";
    public const string SchedulerRead = "scheduler.read";
    public const string SchedulerWrite = "scheduler.write";
    public const string SchedulerExecute = "scheduler.execute";
    public const string SchedulerManage = "scheduler.manage";
    public const string SchedulerPermissionsManage = "scheduler.permissions.manage";
    public const string SchedulerReportsRead = "scheduler.reports.read";
}

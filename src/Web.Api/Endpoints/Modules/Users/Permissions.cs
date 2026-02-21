using Domain.Authorization;

namespace Web.Api.Endpoints.Users;

internal static class Permissions
{
    internal const string UsersAccess = PermissionCodes.UsersAccess;
    internal const string TodosRead = PermissionCodes.TodosRead;
    internal const string TodosWrite = PermissionCodes.TodosWrite;
    internal const string AuthorizationManage = PermissionCodes.AuthorizationManage;
    internal const string ObservabilityRead = PermissionCodes.ObservabilityRead;
    internal const string ObservabilityManage = PermissionCodes.ObservabilityManage;
    internal const string AuditRead = PermissionCodes.AuditRead;
    internal const string AuditManage = PermissionCodes.AuditManage;
    internal const string FilesRead = PermissionCodes.FilesRead;
    internal const string FilesWrite = PermissionCodes.FilesWrite;
    internal const string FilesDelete = PermissionCodes.FilesDelete;
    internal const string FilesShare = PermissionCodes.FilesShare;
    internal const string FilesPermissionsManage = PermissionCodes.FilesPermissionsManage;
    internal const string NotificationsRead = PermissionCodes.NotificationsRead;
    internal const string NotificationsWrite = PermissionCodes.NotificationsWrite;
    internal const string NotificationTemplatesManage = PermissionCodes.NotificationTemplatesManage;
    internal const string NotificationSchedulesManage = PermissionCodes.NotificationSchedulesManage;
    internal const string NotificationPermissionsManage = PermissionCodes.NotificationPermissionsManage;
    internal const string NotificationReportsRead = PermissionCodes.NotificationReportsRead;
    internal const string ProfilesRead = PermissionCodes.ProfilesRead;
    internal const string ProfilesWrite = PermissionCodes.ProfilesWrite;
    internal const string ProfilesPublicRead = PermissionCodes.ProfilesPublicRead;
    internal const string ProfilesAdminRead = PermissionCodes.ProfilesAdminRead;
    internal const string SchedulerRead = PermissionCodes.SchedulerRead;
    internal const string SchedulerWrite = PermissionCodes.SchedulerWrite;
    internal const string SchedulerExecute = PermissionCodes.SchedulerExecute;
    internal const string SchedulerManage = PermissionCodes.SchedulerManage;
    internal const string SchedulerPermissionsManage = PermissionCodes.SchedulerPermissionsManage;
    internal const string SchedulerReportsRead = PermissionCodes.SchedulerReportsRead;
}

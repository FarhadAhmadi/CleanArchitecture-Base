using Domain.Authorization;

namespace Web.Api.Endpoints.Users;

internal static class Permissions
{
    internal const string UsersAccess = PermissionCodes.UsersAccess;
    internal const string TodosRead = PermissionCodes.TodosRead;
    internal const string TodosWrite = PermissionCodes.TodosWrite;
    internal const string AuthorizationManage = PermissionCodes.AuthorizationManage;
}

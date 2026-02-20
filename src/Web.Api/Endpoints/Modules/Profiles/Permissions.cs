using Domain.Authorization;

namespace Web.Api.Endpoints.Profiles;

internal static class Permissions
{
    internal const string ProfilesRead = PermissionCodes.ProfilesRead;
    internal const string ProfilesWrite = PermissionCodes.ProfilesWrite;
    internal const string ProfilesPublicRead = PermissionCodes.ProfilesPublicRead;
    internal const string ProfilesAdminRead = PermissionCodes.ProfilesAdminRead;
}

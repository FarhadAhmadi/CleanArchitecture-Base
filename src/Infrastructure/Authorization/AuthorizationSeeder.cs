using Domain.Authorization;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Authorization;

public sealed class AuthorizationSeeder(
    ApplicationDbContext dbContext,
    AuthorizationBootstrapOptions options)
{
    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        if (!options.SeedDefaults)
        {
            return;
        }

        await SeedPermissionsAsync(cancellationToken);
        await SeedRolesAsync(cancellationToken);
        await SeedRolePermissionsAsync(cancellationToken);
        await SeedUsersAsync(cancellationToken);
        await SeedAdminUserRolesAsync(cancellationToken);
    }

    private async Task SeedPermissionsAsync(CancellationToken cancellationToken)
    {
        Dictionary<string, string> permissions = new(StringComparer.OrdinalIgnoreCase)
        {
            [PermissionCodes.TodosRead] = "Read todo items",
            [PermissionCodes.TodosWrite] = "Create, update, complete and delete todo items",
            [PermissionCodes.UsersAccess] = "Read user profile endpoints",
            [PermissionCodes.AuthorizationManage] = "Manage roles and permissions",
            [PermissionCodes.LoggingEventsWrite] = "Create logging events",
            [PermissionCodes.LoggingEventsRead] = "Read logging events",
            [PermissionCodes.LoggingEventsDelete] = "Delete logging events",
            [PermissionCodes.LoggingAlertsManage] = "Manage logging alert rules",
            [PermissionCodes.LoggingAccessManage] = "Manage logging access control",
            [PermissionCodes.LoggingExportRead] = "Export or read sensitive log reports",
            [PermissionCodes.ObservabilityRead] = "Read operational metrics and SLO dashboards",
            [PermissionCodes.AuditRead] = "Read audit trail data",
            [PermissionCodes.AuditManage] = "Manage audit controls"
        };

        List<string> existingCodes = await dbContext.Permissions
            .Select(p => p.Code)
            .ToListAsync(cancellationToken);

        IEnumerable<Permission> toInsert = permissions
            .Where(x => !existingCodes.Contains(x.Key, StringComparer.OrdinalIgnoreCase))
            .Select(x => new Permission
            {
                Id = Guid.NewGuid(),
                Code = x.Key,
                Description = x.Value
            });

        await dbContext.Permissions.AddRangeAsync(toInsert, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedRolesAsync(CancellationToken cancellationToken)
    {
        string[] roleNames = [options.UserRoleName, options.AdminRoleName, "LogWriter", "LogReader", "Auditor", "SecurityAnalyst"];

        List<string> existing = await dbContext.Roles
            .Select(r => r.Name)
            .ToListAsync(cancellationToken);

        IEnumerable<Role> roles = roleNames
            .Where(name => !existing.Contains(name, StringComparer.OrdinalIgnoreCase))
            .Select(name => new Role
            {
                Id = Guid.NewGuid(),
                Name = name
            });

        await dbContext.Roles.AddRangeAsync(roles, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedRolePermissionsAsync(CancellationToken cancellationToken)
    {
        Role? userRole = await dbContext.Roles.SingleOrDefaultAsync(
            r => r.Name == options.UserRoleName,
            cancellationToken);

        Role? adminRole = await dbContext.Roles.SingleOrDefaultAsync(
            r => r.Name == options.AdminRoleName,
            cancellationToken);

        if (userRole is null || adminRole is null)
        {
            return;
        }

        Dictionary<Guid, HashSet<string>> rolePermissions = new()
        {
            [userRole.Id] = [PermissionCodes.TodosRead, PermissionCodes.TodosWrite],
            [adminRole.Id] =
            [
                PermissionCodes.TodosRead,
                PermissionCodes.TodosWrite,
                PermissionCodes.UsersAccess,
                PermissionCodes.AuthorizationManage,
                PermissionCodes.LoggingEventsWrite,
                PermissionCodes.LoggingEventsRead,
                PermissionCodes.LoggingEventsDelete,
                PermissionCodes.LoggingAlertsManage,
                PermissionCodes.LoggingAccessManage,
                PermissionCodes.LoggingExportRead,
                PermissionCodes.ObservabilityRead,
                PermissionCodes.AuditRead,
                PermissionCodes.AuditManage
            ]
        };

        Dictionary<string, Guid> roleIdByName = await dbContext.Roles
            .ToDictionaryAsync(x => x.Name, x => x.Id, StringComparer.OrdinalIgnoreCase, cancellationToken);

        if (roleIdByName.TryGetValue("LogWriter", out Guid logWriterRoleId))
        {
            rolePermissions[logWriterRoleId] = [PermissionCodes.LoggingEventsWrite];
        }

        if (roleIdByName.TryGetValue("LogReader", out Guid logReaderRoleId))
        {
            rolePermissions[logReaderRoleId] = [PermissionCodes.LoggingEventsRead];
        }

        if (roleIdByName.TryGetValue("Auditor", out Guid auditorRoleId))
        {
            rolePermissions[auditorRoleId] = [PermissionCodes.LoggingEventsRead, PermissionCodes.LoggingExportRead];
        }

        if (roleIdByName.TryGetValue("SecurityAnalyst", out Guid securityRoleId))
        {
            rolePermissions[securityRoleId] = [PermissionCodes.LoggingEventsRead, PermissionCodes.LoggingAlertsManage];
        }

        Dictionary<string, Guid> permissionIdByCode = await dbContext.Permissions
            .ToDictionaryAsync(x => x.Code, x => x.Id, StringComparer.OrdinalIgnoreCase, cancellationToken);

        List<RolePermission> existing = await dbContext.RolePermissions.ToListAsync(cancellationToken);
        HashSet<string> existingKeys = [.. existing.Select(x => $"{x.RoleId}:{x.PermissionId}")];

        List<RolePermission> toInsert = [];

        foreach ((Guid roleId, HashSet<string> codes) in rolePermissions)
        {
            foreach (string code in codes)
            {
                if (!permissionIdByCode.TryGetValue(code, out Guid permissionId))
                {
                    continue;
                }

                string key = $"{roleId}:{permissionId}";
                if (existingKeys.Contains(key))
                {
                    continue;
                }

                toInsert.Add(new RolePermission
                {
                    RoleId = roleId,
                    PermissionId = permissionId
                });
            }
        }

        await dbContext.RolePermissions.AddRangeAsync(toInsert, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedUsersAsync(CancellationToken cancellationToken)
    {
        Role? userRole = await dbContext.Roles.SingleOrDefaultAsync(
            r => r.Name == options.UserRoleName,
            cancellationToken);

        if (userRole is null)
        {
            return;
        }

        List<Guid> userIds = await dbContext.Users.Select(u => u.Id).ToListAsync(cancellationToken);
        List<Guid> assignedUserIds = await dbContext.UserRoles
            .Where(ur => ur.RoleId == userRole.Id)
            .Select(ur => ur.UserId)
            .ToListAsync(cancellationToken);

        IEnumerable<UserRole> defaultAssignments = userIds
            .Where(id => !assignedUserIds.Contains(id))
            .Select(id => new UserRole
            {
                UserId = id,
                RoleId = userRole.Id
            });

        await dbContext.UserRoles.AddRangeAsync(defaultAssignments, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedAdminUserRolesAsync(CancellationToken cancellationToken)
    {
        if (options.AdminEmails.Length == 0)
        {
            return;
        }

        Role? adminRole = await dbContext.Roles.SingleOrDefaultAsync(
            r => r.Name == options.AdminRoleName,
            cancellationToken);

        if (adminRole is null)
        {
            return;
        }

        List<Guid> adminUserIds = await dbContext.Users
            .Where(u => options.AdminEmails.Contains(u.Email))
            .Select(u => u.Id)
            .ToListAsync(cancellationToken);

        List<Guid> alreadyAssigned = await dbContext.UserRoles
            .Where(ur => ur.RoleId == adminRole.Id)
            .Select(ur => ur.UserId)
            .ToListAsync(cancellationToken);

        IEnumerable<UserRole> toInsert = adminUserIds
            .Where(id => !alreadyAssigned.Contains(id))
            .Select(id => new UserRole
            {
                UserId = id,
                RoleId = adminRole.Id
            });

        await dbContext.UserRoles.AddRangeAsync(toInsert, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

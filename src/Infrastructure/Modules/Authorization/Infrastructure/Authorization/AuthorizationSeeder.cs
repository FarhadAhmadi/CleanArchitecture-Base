using Domain.Authorization;
using Domain.Users;
using Infrastructure.Database;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Authorization;

public sealed class AuthorizationSeeder(
    ApplicationDbContext dbContext,
    AuthorizationBootstrapOptions options,
    UserManager<User> userManager)
{
    private const string BootstrapAdminEmail = "farhad@gmail.com";
    private const string BootstrapAdminPassword = "12345678";

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
        await SeedBootstrapAdminAsync(cancellationToken);
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
            [PermissionCodes.ObservabilityManage] = "Manage observability operational controls and replays",
            [PermissionCodes.AuditRead] = "Read audit trail data",
            [PermissionCodes.AuditManage] = "Manage audit controls",
            [PermissionCodes.FilesRead] = "Read file metadata and file content",
            [PermissionCodes.FilesWrite] = "Upload and update files",
            [PermissionCodes.FilesDelete] = "Delete files",
            [PermissionCodes.FilesShare] = "Generate secure temporary links for file sharing",
            [PermissionCodes.FilesPermissionsManage] = "Manage per-file ACL permissions",
            [PermissionCodes.NotificationsRead] = "Read notifications and delivery states",
            [PermissionCodes.NotificationsWrite] = "Create and dispatch notifications",
            [PermissionCodes.NotificationTemplatesManage] = "Create/update/delete notification templates",
            [PermissionCodes.NotificationSchedulesManage] = "Manage scheduled notifications",
            [PermissionCodes.NotificationPermissionsManage] = "Manage notification ACL permissions",
            [PermissionCodes.NotificationReportsRead] = "Read notification reports",
            [PermissionCodes.ProfilesRead] = "Read own profile data",
            [PermissionCodes.ProfilesWrite] = "Create/update own profile data",
            [PermissionCodes.ProfilesPublicRead] = "Read public profile information",
            [PermissionCodes.ProfilesAdminRead] = "Read profile analytics and admin reports",
            [PermissionCodes.SchedulerRead] = "Read scheduler jobs and runtime states",
            [PermissionCodes.SchedulerWrite] = "Create and update scheduler jobs and schedules",
            [PermissionCodes.SchedulerExecute] = "Execute scheduler jobs manually",
            [PermissionCodes.SchedulerManage] = "Manage scheduler control operations (pause/resume/disable)",
            [PermissionCodes.SchedulerPermissionsManage] = "Manage scheduler job ACL permissions",
            [PermissionCodes.SchedulerReportsRead] = "Read and export scheduler execution reports"
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

        List<Role> existingRoles = await dbContext.Roles.ToListAsync(cancellationToken);
        var existing = existingRoles
            .Where(r => r.Name != null)
            .Select(r => r.Name!)
            .ToList();

        IEnumerable<Role> roles = roleNames
            .Where(name => !existing.Contains(name, StringComparer.OrdinalIgnoreCase))
            .Select(name => new Role
            {
                Id = Guid.NewGuid(),
                Name = name,
                NormalizedName = name.ToUpperInvariant(),
                ConcurrencyStamp = Guid.NewGuid().ToString("N")
            });

        await dbContext.Roles.AddRangeAsync(roles, cancellationToken);

        // Repair legacy roles created before Identity migration.
        foreach (Role role in existingRoles)
        {
            if (string.IsNullOrWhiteSpace(role.Name))
            {
                continue;
            }

            bool changed = false;

            if (string.IsNullOrWhiteSpace(role.NormalizedName))
            {
                role.NormalizedName = role.Name.ToUpperInvariant();
                changed = true;
            }

            if (string.IsNullOrWhiteSpace(role.ConcurrencyStamp))
            {
                role.ConcurrencyStamp = Guid.NewGuid().ToString("N");
                changed = true;
            }

            if (changed)
            {
                dbContext.Roles.Update(role);
            }
        }

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
            [userRole.Id] =
            [
                PermissionCodes.TodosRead,
                PermissionCodes.TodosWrite,
                PermissionCodes.ProfilesRead,
                PermissionCodes.ProfilesWrite,
                PermissionCodes.ProfilesPublicRead
            ],
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
                PermissionCodes.ObservabilityManage,
                PermissionCodes.AuditRead,
                PermissionCodes.AuditManage,
                PermissionCodes.FilesRead,
                PermissionCodes.FilesWrite,
                PermissionCodes.FilesDelete,
                PermissionCodes.FilesShare,
                PermissionCodes.FilesPermissionsManage,
                PermissionCodes.NotificationsRead,
                PermissionCodes.NotificationsWrite,
                PermissionCodes.NotificationTemplatesManage,
                PermissionCodes.NotificationSchedulesManage,
                PermissionCodes.NotificationPermissionsManage,
                PermissionCodes.NotificationReportsRead,
                PermissionCodes.ProfilesRead,
                PermissionCodes.ProfilesWrite,
                PermissionCodes.ProfilesPublicRead,
                PermissionCodes.ProfilesAdminRead,
                PermissionCodes.SchedulerRead,
                PermissionCodes.SchedulerWrite,
                PermissionCodes.SchedulerExecute,
                PermissionCodes.SchedulerManage,
                PermissionCodes.SchedulerPermissionsManage,
                PermissionCodes.SchedulerReportsRead
            ]
        };

        Dictionary<string, Guid> roleIdByName = await dbContext.Roles
            .Where(x => x.Name != null)
            .ToDictionaryAsync(x => x.Name!, x => x.Id, StringComparer.OrdinalIgnoreCase, cancellationToken);

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
            .Where(u => u.Email != null && options.AdminEmails.Contains(u.Email))
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

    private async Task SeedBootstrapAdminAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        Role? adminRole = await dbContext.Roles.SingleOrDefaultAsync(
            r => r.Name == options.AdminRoleName,
            cancellationToken);

        if (adminRole is null || string.IsNullOrWhiteSpace(adminRole.Name))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(adminRole.NormalizedName))
        {
            adminRole.NormalizedName = adminRole.Name.ToUpperInvariant();
            adminRole.ConcurrencyStamp ??= Guid.NewGuid().ToString("N");
            dbContext.Roles.Update(adminRole);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        User? user = await userManager.FindByEmailAsync(BootstrapAdminEmail);

        if (user is null)
        {
            user = new User
            {
                Id = Guid.NewGuid(),
                Email = BootstrapAdminEmail,
                UserName = BootstrapAdminEmail,
                NormalizedEmail = BootstrapAdminEmail.ToUpperInvariant(),
                NormalizedUserName = BootstrapAdminEmail.ToUpperInvariant(),
                EmailConfirmed = true,
                FirstName = "Farhad",
                LastName = "Admin",
                LockoutEnabled = true,
                SecurityStamp = Guid.NewGuid().ToString("N")
            };

            IdentityResult createResult = await userManager.CreateAsync(user, BootstrapAdminPassword);
            if (!createResult.Succeeded)
            {
                return;
            }
        }

        if (!await userManager.IsInRoleAsync(user, adminRole.Name))
        {
            await userManager.AddToRoleAsync(user, adminRole.Name);
        }
    }
}

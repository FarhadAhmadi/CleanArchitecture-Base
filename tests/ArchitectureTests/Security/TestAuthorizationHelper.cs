using Domain.Authorization;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ArchitectureTests.Security;

internal static class TestAuthorizationHelper
{
    internal static async Task EnsureRoleWithPermissionsAsync(
        ApiWebApplicationFactory factory,
        string roleName,
        params string[] permissionCodes)
    {
        using IServiceScope scope = factory.Services.CreateScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        Role? role = await dbContext.Roles.SingleOrDefaultAsync(x => x.Name == roleName);
        if (role is null)
        {
            role = new Role { Id = Guid.NewGuid(), Name = roleName };
            dbContext.Roles.Add(role);
        }

        List<Permission> permissions = await dbContext.Permissions
            .Where(x => permissionCodes.Contains(x.Code))
            .ToListAsync();

        foreach (string code in permissionCodes)
        {
            if (permissions.All(x => x.Code != code))
            {
                Permission permission = new()
                {
                    Id = Guid.NewGuid(),
                    Code = code,
                    Description = code
                };

                dbContext.Permissions.Add(permission);
                permissions.Add(permission);
            }
        }

        foreach (Permission permission in permissions)
        {
            bool hasRolePermission = await dbContext.RolePermissions
                .AnyAsync(x => x.RoleId == role.Id && x.PermissionId == permission.Id);

            if (!hasRolePermission)
            {
                dbContext.RolePermissions.Add(new RolePermission
                {
                    RoleId = role.Id,
                    PermissionId = permission.Id
                });
            }
        }

        await dbContext.SaveChangesAsync();
    }
}

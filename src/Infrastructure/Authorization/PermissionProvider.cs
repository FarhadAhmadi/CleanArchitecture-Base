using Application.Abstractions.Data;
using Application.Abstractions.Authorization;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Infrastructure.Authorization;

internal sealed class PermissionProvider(
    IApplicationReadDbContext context,
    IDistributedCache cache,
    IPermissionCacheVersionService versionService,
    PermissionCacheOptions options)
{
    public async Task<HashSet<string>> GetForUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        long version = await versionService.GetVersionAsync(cancellationToken);
        string cacheKey = $"authorization:permissions:v{version}:user:{userId:N}";

        byte[]? cachedBytes = await cache.GetAsync(cacheKey, cancellationToken);
        if (cachedBytes is not null)
        {
            string[]? cachedPermissions = JsonSerializer.Deserialize<string[]>(cachedBytes);
            if (cachedPermissions is not null)
            {
                return [.. cachedPermissions.Distinct(StringComparer.OrdinalIgnoreCase)];
            }
        }

        List<string> directPermissions = await (
            from userPermission in context.UserPermissions
            join permission in context.Permissions on userPermission.PermissionId equals permission.Id
            where userPermission.UserId == userId
            select permission.Code)
            .ToListAsync(cancellationToken);

        List<string> rolePermissions = await (
            from userRole in context.UserRoles
            join rolePermission in context.RolePermissions on userRole.RoleId equals rolePermission.RoleId
            join permission in context.Permissions on rolePermission.PermissionId equals permission.Id
            where userRole.UserId == userId
            select permission.Code)
            .ToListAsync(cancellationToken);

        string[] permissions = [.. directPermissions.Concat(rolePermissions).Distinct(StringComparer.OrdinalIgnoreCase)];

        DistributedCacheEntryOptions cacheOptions = new()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(Math.Max(30, options.AbsoluteExpirationSeconds))
        };

        byte[] payload = JsonSerializer.SerializeToUtf8Bytes(permissions);
        await cache.SetAsync(cacheKey, payload, cacheOptions, cancellationToken);

        return [.. permissions];
    }
}

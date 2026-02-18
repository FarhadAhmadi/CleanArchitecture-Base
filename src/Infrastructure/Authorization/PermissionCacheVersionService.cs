using System.Globalization;
using System.Text;
using Application.Abstractions.Authorization;
using Microsoft.Extensions.Caching.Distributed;

namespace Infrastructure.Authorization;

internal sealed class PermissionCacheVersionService(
    IDistributedCache cache,
    PermissionCacheOptions options) : IPermissionCacheVersionService
{
    private const string VersionKey = "authorization:permissions:version";

    public async Task<long> GetVersionAsync(CancellationToken cancellationToken)
    {
        byte[]? bytes = await cache.GetAsync(VersionKey, cancellationToken);
        if (bytes is null || bytes.Length == 0)
        {
            await SetVersionAsync(1, cancellationToken);
            return 1;
        }

        if (!long.TryParse(Encoding.UTF8.GetString(bytes), out long value))
        {
            value = 1;
            await SetVersionAsync(value, cancellationToken);
        }

        return value;
    }

    public async Task BumpVersionAsync(CancellationToken cancellationToken)
    {
        long current = await GetVersionAsync(cancellationToken);
        await SetVersionAsync(current + 1, cancellationToken);
    }

    private Task SetVersionAsync(long value, CancellationToken cancellationToken)
    {
        DistributedCacheEntryOptions entryOptions = new()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(Math.Max(30, options.AbsoluteExpirationSeconds * 2))
        };

        return cache.SetAsync(
            VersionKey,
            Encoding.UTF8.GetBytes(value.ToString(CultureInfo.InvariantCulture)),
            entryOptions,
            cancellationToken);
    }
}

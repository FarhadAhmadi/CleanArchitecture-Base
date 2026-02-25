using Application.Abstractions.Authorization;
using Infrastructure.Authorization;
using Infrastructure.Caching;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

internal static class CachingModule
{
    internal static IServiceCollection AddCachingModule(this IServiceCollection services, IConfiguration configuration)
    {
        RedisCacheOptions redisOptions = configuration
            .GetSection(RedisCacheOptions.SectionName)
            .Get<RedisCacheOptions>() ?? new RedisCacheOptions();

        PermissionCacheOptions permissionCacheOptions = configuration
            .GetSection(PermissionCacheOptions.SectionName)
            .Get<PermissionCacheOptions>() ?? new PermissionCacheOptions();

        services.AddSingleton(redisOptions);
        services.AddSingleton(permissionCacheOptions);

        if (redisOptions.Enabled && !string.IsNullOrWhiteSpace(redisOptions.ConnectionString))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisOptions.ConnectionString;
                options.InstanceName = redisOptions.InstanceName;
            });
        }
        else
        {
            services.AddDistributedMemoryCache();
        }

        services.AddSingleton<IPermissionCacheVersionService, PermissionCacheVersionService>();

        return services;
    }
}

using Infrastructure.Integration;
using Infrastructure.Files;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

internal static class HealthChecksModule
{
    private static readonly string[] ReadyTag = ["ready"];

    internal static IServiceCollection AddHealthChecksModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddHealthChecks()
            .AddSqlServer(configuration.GetConnectionString("Database")!)
            .AddCheck<OutboxHealthCheck>("outbox", tags: ReadyTag)
            .AddCheck<FileStorageHealthCheck>("file-storage", tags: ReadyTag);

        return services;
    }
}

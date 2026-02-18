using Infrastructure.Integration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

internal static class HealthChecksModule
{
    internal static IServiceCollection AddHealthChecksModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddHealthChecks()
            .AddSqlServer(configuration.GetConnectionString("Database")!)
            .AddCheck<OutboxHealthCheck>("outbox");

        return services;
    }
}

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        return services
            .AddCoreModule()
            .AddDataAccessModule(configuration)
            .AddCachingModule(configuration)
            .AddIntegrationModule(configuration)
            .AddHealthChecksModule(configuration)
            .AddLoggingModule(configuration)
            .AddUsersModule(configuration)
            .AddAuthModule(configuration)
            .AddAuthorizationModule(configuration)
            .AddAuditModule()
            .AddMonitoringModule()
            .AddFilesModule(configuration)
            .AddNotificationsModule(configuration);
    }
}

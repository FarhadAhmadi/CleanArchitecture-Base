using Infrastructure.Monitoring;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

internal static class MonitoringModule
{
    internal static IServiceCollection AddMonitoringModule(this IServiceCollection services)
    {
        services.AddScoped<OperationalMetricsService>();
        services.AddScoped<OrchestrationHealthService>();
        services.AddScoped<OrchestrationReplayService>();
        services.AddSingleton<EventContractCatalogService>();
        return services;
    }
}

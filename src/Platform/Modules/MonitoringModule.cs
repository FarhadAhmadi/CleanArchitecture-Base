using Application.Abstractions.Observability;
using Infrastructure.Monitoring;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

internal static class MonitoringModule
{
    internal static IServiceCollection AddMonitoringModule(this IServiceCollection services)
    {
        services.AddScoped<IOperationalMetricsService, OperationalMetricsService>();
        services.AddScoped<IOrchestrationHealthService, OrchestrationHealthService>();
        services.AddScoped<IOrchestrationReplayService, OrchestrationReplayService>();
        services.AddSingleton<IEventContractCatalogService, EventContractCatalogService>();
        return services;
    }
}

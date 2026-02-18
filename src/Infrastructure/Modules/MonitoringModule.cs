using Infrastructure.Monitoring;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

internal static class MonitoringModule
{
    internal static IServiceCollection AddMonitoringModule(this IServiceCollection services)
    {
        services.AddScoped<OperationalMetricsService>();
        return services;
    }
}

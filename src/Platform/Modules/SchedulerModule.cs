using Application.Abstractions.Scheduler;
using Infrastructure.Scheduler;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

internal static class SchedulerModule
{
    internal static IServiceCollection AddSchedulerModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        SchedulerOptions options = configuration
            .GetSection(SchedulerOptions.SectionName)
            .Get<SchedulerOptions>() ?? new SchedulerOptions();

        services.AddSingleton(options);
        services.AddSingleton<SchedulerMetrics>();
        services.AddScoped<ISchedulerPayloadValidator, SchedulerPayloadValidator>();
        services.AddScoped<ISchedulerRetryPolicyProvider, SchedulerRetryPolicyProvider>();
        services.AddScoped<ISchedulerDistributedLockProvider, DatabaseSchedulerDistributedLockProvider>();
        services.AddScoped<ISchedulerExecutionService, SchedulerExecutionService>();
        services.AddScoped<IScheduledJobHandler, GenericNoOpScheduledJobHandler>();
        services.AddScoped<IScheduledJobHandler, CleanupOldSchedulerExecutionsJobHandler>();
        services.AddScoped<IScheduledJobHandler, NotificationDispatchProbeJobHandler>();
        services.AddScoped<SchedulerDataSeeder>();
        services.AddHostedService<SchedulerWorker>();
        return services;
    }
}

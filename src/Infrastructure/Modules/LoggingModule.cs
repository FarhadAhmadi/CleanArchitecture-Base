using Application.Abstractions.Logging;
using Infrastructure.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

internal static class LoggingModule
{
    internal static IServiceCollection AddLoggingModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        LoggingOptions options = configuration
            .GetSection(LoggingOptions.SectionName)
            .Get<LoggingOptions>() ?? new LoggingOptions();

        services.AddSingleton(options);
        services.AddSingleton<ILogSanitizer, LogSanitizer>();
        services.AddSingleton<ILogIntegrityService, LogIntegrityService>();
        services.AddSingleton<ILogIngestionQueue, LogIngestionQueue>();
        services.AddSingleton<IAlertDispatchQueue, AlertDispatchQueue>();
        services.AddScoped<IAlertIncidentDispatchScheduler, AlertIncidentDispatchScheduler>();
        services.AddScoped<ILogIngestionService, LogIngestionService>();
        services.AddSingleton<ILoggingHealthService, LoggingHealthService>();
        services.AddHostedService<LogRetryWorker>();
        services.AddHostedService<AlertDispatchWorker>();

        return services;
    }
}

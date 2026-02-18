using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Notifications;

internal sealed class NotificationDispatchWorker(
    NotificationOptions options,
    IServiceScopeFactory scopeFactory,
    ILogger<NotificationDispatchWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.Enabled)
        {
            logger.LogInformation("Notification dispatch worker is disabled.");
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using IServiceScope scope = scopeFactory.CreateScope();
                NotificationDispatcher dispatcher = scope.ServiceProvider.GetRequiredService<NotificationDispatcher>();
                int processed = await dispatcher.DispatchPendingAsync(stoppingToken);
                if (processed > 0 && logger.IsEnabled(LogLevel.Information))
                {
                    logger.LogInformation("Notification dispatch cycle processed {Processed} items.", processed);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Notification dispatch worker failed.");
            }

            await Task.Delay(TimeSpan.FromSeconds(Math.Max(1, options.DispatchPollingSeconds)), stoppingToken);
        }
    }
}

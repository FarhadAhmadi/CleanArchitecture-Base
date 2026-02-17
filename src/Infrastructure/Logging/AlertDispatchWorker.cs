using Domain.Logging;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace Infrastructure.Logging;

internal sealed class AlertDispatchWorker(
    IAlertDispatchQueue queue,
    IServiceScopeFactory scopeFactory,
    ILogger<AlertDispatchWorker> logger) : BackgroundService
{
    private readonly ResiliencePipeline _retryPipeline = new ResiliencePipelineBuilder()
        .AddRetry(new RetryStrategyOptions
        {
            ShouldHandle = new PredicateBuilder().Handle<Exception>(),
            MaxRetryAttempts = 3,
            BackoffType = DelayBackoffType.Exponential,
            Delay = TimeSpan.FromMilliseconds(250)
        })
        .Build();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            Guid incidentId = await queue.DequeueAsync(stoppingToken);

            using IServiceScope scope = scopeFactory.CreateScope();
            ApplicationDbContext db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            AlertIncident? incident = await db.AlertIncidents.SingleOrDefaultAsync(x => x.Id == incidentId, stoppingToken);
            if (incident is null)
            {
                continue;
            }

            try
            {
                await _retryPipeline.ExecuteAsync(async token =>
                {
                    incident.Status = "Delivered";
                    incident.LastError = null;
                    await db.SaveChangesAsync(token);
                }, stoppingToken);
            }
            catch (Exception ex)
            {
                incident.RetryCount++;
                incident.Status = "Retry";
                incident.LastError = ex.Message;
                incident.NextRetryAtUtc = DateTime.UtcNow.AddSeconds(Math.Pow(2, Math.Min(incident.RetryCount, 8)));
                await db.SaveChangesAsync(stoppingToken);
                logger.LogError(ex, "Alert dispatch failed for incident {IncidentId}", incidentId);
            }
        }
    }
}

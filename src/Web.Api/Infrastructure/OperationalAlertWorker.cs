using System.Text;
using System.Text.Json;
using Infrastructure.Monitoring;

namespace Web.Api.Infrastructure;

internal sealed class OperationalAlertWorker(
    OperationalAlertingOptions alertingOptions,
    OperationalSloOptions sloOptions,
    IServiceScopeFactory scopeFactory,
    IHttpClientFactory httpClientFactory,
    ILogger<OperationalAlertWorker> logger) : BackgroundService
{
    private readonly Dictionary<string, DateTime> _lastAlertUtc = new(StringComparer.OrdinalIgnoreCase);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!alertingOptions.Enabled)
        {
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using IServiceScope scope = scopeFactory.CreateScope();
                OperationalMetricsService metricsService = scope.ServiceProvider.GetRequiredService<OperationalMetricsService>();
                OperationalMetricsSnapshot snapshot = await metricsService.GetSnapshotAsync(stoppingToken);
                List<string> incidents = Evaluate(snapshot);

                foreach (string incident in incidents)
                {
                    if (!CanSend(incident))
                    {
                        continue;
                    }

                    await NotifyAsync(incident, snapshot, stoppingToken);
                    _lastAlertUtc[incident] = DateTime.UtcNow;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Operational alert worker failed.");
            }

            await Task.Delay(TimeSpan.FromSeconds(Math.Max(10, alertingOptions.PollingIntervalSeconds)), stoppingToken);
        }
    }

    private List<string> Evaluate(OperationalMetricsSnapshot snapshot)
    {
        List<string> incidents = [];

        double corruptedRate = snapshot.TotalLogEvents == 0
            ? 0
            : (double)snapshot.CorruptedLogEvents / snapshot.TotalLogEvents * 100.0;

        if (corruptedRate > sloOptions.MaxCorruptedLogRatePercent)
        {
            incidents.Add($"Corrupted log rate breach: {corruptedRate:F4}% > {sloOptions.MaxCorruptedLogRatePercent}%");
        }

        if (snapshot.OutboxPending > sloOptions.MaxOutboxPending)
        {
            incidents.Add($"Outbox backlog breach: {snapshot.OutboxPending} > {sloOptions.MaxOutboxPending}");
        }

        if (snapshot.OutboxFailed > sloOptions.MaxOutboxFailed)
        {
            incidents.Add($"Outbox failed breach: {snapshot.OutboxFailed} > {sloOptions.MaxOutboxFailed}");
        }

        if (snapshot.IngestionQueueDepth > sloOptions.MaxIngestionQueueDepth)
        {
            incidents.Add($"Ingestion queue breach: {snapshot.IngestionQueueDepth} > {sloOptions.MaxIngestionQueueDepth}");
        }

        return incidents;
    }

    private bool CanSend(string incident)
    {
        if (!_lastAlertUtc.TryGetValue(incident, out DateTime last))
        {
            return true;
        }

        return DateTime.UtcNow - last >= TimeSpan.FromMinutes(Math.Max(1, alertingOptions.CooldownMinutes));
    }

    private async Task NotifyAsync(string incident, OperationalMetricsSnapshot snapshot, CancellationToken cancellationToken)
    {
        HttpClient client = httpClientFactory.CreateClient("default");
        string runbookLink = string.IsNullOrWhiteSpace(alertingOptions.RunbookBaseUrl)
            ? string.Empty
            : $" | runbook={alertingOptions.RunbookBaseUrl!.TrimEnd('/')}/operations/orchestration-alerts";
        string message =
            $"[Operational Alert] {incident} | timestamp={snapshot.TimestampUtc:O} | outboxPending={snapshot.OutboxPending} | outboxFailed={snapshot.OutboxFailed} | ingestionQueueDepth={snapshot.IngestionQueueDepth}{runbookLink}";

        foreach (string webhookUrl in alertingOptions.WebhookUrls.Where(x => !string.IsNullOrWhiteSpace(x)))
        {
            using StringContent payload = new(
                JsonSerializer.Serialize(new { text = message }),
                Encoding.UTF8,
                "application/json");

            HttpResponseMessage response = await client.PostAsync(webhookUrl, payload, cancellationToken);
            response.EnsureSuccessStatusCode();
        }

        if (!string.IsNullOrWhiteSpace(alertingOptions.PagerDutyRoutingKey))
        {
            var pagerDutyPayload = new
            {
                routing_key = alertingOptions.PagerDutyRoutingKey,
                event_action = "trigger",
                payload = new
                {
                    summary = incident,
                    severity = "error",
                    source = "CleanArchitecture.WebApi",
                    timestamp = snapshot.TimestampUtc.ToString("O")
                }
            };

            using StringContent pagerDutyContent = new(
                JsonSerializer.Serialize(pagerDutyPayload),
                Encoding.UTF8,
                "application/json");

            HttpResponseMessage response = await client.PostAsync(
                alertingOptions.PagerDutyEventsApiUrl,
                pagerDutyContent,
                cancellationToken);

            response.EnsureSuccessStatusCode();
        }

        logger.LogWarning("Operational alert sent. Message={Message}", incident);
    }
}

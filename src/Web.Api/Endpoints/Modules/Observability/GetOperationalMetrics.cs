using Infrastructure.Monitoring;
using Web.Api.Endpoints.Users;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Observability;

internal sealed class GetOperationalMetrics : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("observability/metrics", async (
            OperationalMetricsService metricsService,
            OperationalSloOptions sloOptions,
            CancellationToken cancellationToken) =>
        {
            OperationalMetricsSnapshot snapshot = await metricsService.GetSnapshotAsync(cancellationToken);

            double corruptedRate = snapshot.TotalLogEvents == 0
                ? 0
                : (double)snapshot.CorruptedLogEvents / snapshot.TotalLogEvents * 100.0;

            bool isHealthy = corruptedRate <= sloOptions.MaxCorruptedLogRatePercent &&
                             snapshot.OutboxPending <= sloOptions.MaxOutboxPending &&
                             snapshot.OutboxFailed <= sloOptions.MaxOutboxFailed &&
                             snapshot.IngestionQueueDepth <= sloOptions.MaxIngestionQueueDepth;

            return Results.Ok(new
            {
                status = isHealthy ? "Healthy" : "Degraded",
                slo = new
                {
                    maxCorruptedLogRatePercent = sloOptions.MaxCorruptedLogRatePercent,
                    maxOutboxPending = sloOptions.MaxOutboxPending,
                    maxOutboxFailed = sloOptions.MaxOutboxFailed,
                    maxIngestionQueueDepth = sloOptions.MaxIngestionQueueDepth
                },
                values = new
                {
                    corruptedLogRatePercent = Math.Round(corruptedRate, 4),
                    snapshot.OutboxPending,
                    snapshot.OutboxFailed,
                    snapshot.IngestionQueueDepth,
                    snapshot.IngestionDropped,
                    snapshot.AlertQueueDepth,
                    snapshot.TotalLogEvents,
                    snapshot.CorruptedLogEvents,
                    snapshot.TimestampUtc
                }
            });
        })
        .WithTags("Observability")
        .HasPermission(Permissions.ObservabilityRead);
    }
}

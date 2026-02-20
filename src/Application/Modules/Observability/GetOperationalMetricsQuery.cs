using Application.Abstractions.Messaging;
using Application.Abstractions.Observability;

namespace Application.Observability;

public sealed record GetOperationalMetricsQuery() : IQuery<IResult>;
internal sealed class GetOperationalMetricsQueryHandler(
    IOperationalMetricsService metricsService,
    OperationalSloOptions sloOptions) : ResultWrappingQueryHandler<GetOperationalMetricsQuery>
{
    protected override async Task<IResult> HandleCore(GetOperationalMetricsQuery query, CancellationToken cancellationToken)
    {
        _ = query;
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
    }
}





using Infrastructure.Monitoring;
using Web.Api.Endpoints.Users;
using Web.Api.Extensions;

namespace Web.Api.Endpoints.Observability;

internal sealed class GetOrchestrationHealth : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("dashboard/orchestration-health", async (
            OrchestrationHealthService healthService,
            CancellationToken cancellationToken) =>
        {
            OrchestrationHealthSnapshot snapshot = await healthService.GetSnapshotAsync(cancellationToken);

            return Results.Ok(new
            {
                snapshot.Status,
                snapshot.TimestampUtc,
                backlogAges = new
                {
                    outboxSeconds = ToBacklogSeconds(snapshot.TimestampUtc, snapshot.OldestOutboxPendingUtc),
                    inboxSeconds = ToBacklogSeconds(snapshot.TimestampUtc, snapshot.OldestInboxPendingUtc),
                    notificationsSeconds = ToBacklogSeconds(snapshot.TimestampUtc, snapshot.OldestNotificationPendingUtc)
                },
                orchestration = new
                {
                    outbox = new
                    {
                        pending = snapshot.OutboxPending,
                        failed = snapshot.OutboxFailed,
                        status = snapshot.OutboxFailed == 0 ? "Healthy" : "Degraded"
                    },
                    inbox = new
                    {
                        pending = snapshot.InboxPending,
                        failed = snapshot.InboxFailed,
                        status = snapshot.InboxFailed == 0 ? "Healthy" : "Degraded"
                    },
                    alerts = new
                    {
                        queued = snapshot.AlertsQueued,
                        failed = snapshot.AlertsFailed,
                        status = snapshot.AlertsFailed == 0 ? "Healthy" : "Degraded"
                    },
                    notifications = new
                    {
                        pending = snapshot.NotificationsPending,
                        failed = snapshot.NotificationsFailed,
                        status = snapshot.NotificationsFailed == 0 ? "Healthy" : "Degraded"
                    }
                }
            });
        })
        .WithTags("Observability")
        .HasPermission(Permissions.ObservabilityRead);
    }

    private static int ToBacklogSeconds(DateTime nowUtc, DateTime? oldestUtc)
    {
        if (!oldestUtc.HasValue)
        {
            return 0;
        }

        return Math.Max(0, (int)(nowUtc - oldestUtc.Value).TotalSeconds);
    }
}

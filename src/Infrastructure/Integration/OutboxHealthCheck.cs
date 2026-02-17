using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Infrastructure.Integration;

internal sealed class OutboxHealthCheck(
    ApplicationDbContext dbContext,
    OutboxOptions options) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        int pending = await dbContext.Set<OutboxMessage>()
            .AsNoTracking()
            .CountAsync(x => x.ProcessedOnUtc == null && x.RetryCount < options.MaxRetryCount, cancellationToken);

        int failed = await dbContext.Set<OutboxMessage>()
            .AsNoTracking()
            .CountAsync(x => x.ProcessedOnUtc == null && x.RetryCount >= options.MaxRetryCount, cancellationToken);

        if (failed > 0)
        {
            return HealthCheckResult.Unhealthy("Outbox has failed messages.", data: new Dictionary<string, object>
            {
                ["pending"] = pending,
                ["failed"] = failed
            });
        }

        if (pending > options.BatchSize * 20)
        {
            return HealthCheckResult.Degraded("Outbox backlog is high.", data: new Dictionary<string, object>
            {
                ["pending"] = pending
            });
        }

        return HealthCheckResult.Healthy("Outbox is healthy.", new Dictionary<string, object>
        {
            ["pending"] = pending,
            ["failed"] = failed
        });
    }
}

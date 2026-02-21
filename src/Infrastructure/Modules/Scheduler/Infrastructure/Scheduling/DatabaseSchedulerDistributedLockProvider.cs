using Application.Abstractions.Scheduler;
using Domain.Modules.Scheduler;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Scheduler;

internal sealed class DatabaseSchedulerDistributedLockProvider(
    ApplicationDbContext dbContext,
    SchedulerOptions options,
    ILogger<DatabaseSchedulerDistributedLockProvider> logger) : ISchedulerDistributedLockProvider
{
    private readonly string _ownerId = (string.IsNullOrWhiteSpace(options.NodeId)
        ? $"{Environment.MachineName}:{Environment.ProcessId}"
        : options.NodeId.Trim()) + ":" + Guid.NewGuid().ToString("N");

    public async Task<bool> TryAcquireAsync(string lockName, TimeSpan leaseDuration, CancellationToken cancellationToken)
    {
        DateTime nowUtc = DateTime.UtcNow;
        DateTime expiresAtUtc = nowUtc.Add(leaseDuration);

        SchedulerLockLease? existing = await dbContext.SchedulerLockLeases
            .SingleOrDefaultAsync(x => x.LockName == lockName, cancellationToken);

        if (existing is null)
        {
            dbContext.SchedulerLockLeases.Add(new SchedulerLockLease
            {
                LockName = lockName,
                OwnerNodeId = _ownerId,
                AcquiredAtUtc = nowUtc,
                ExpiresAtUtc = expiresAtUtc
            });

            try
            {
                await dbContext.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (DbUpdateException ex)
            {
                if (logger.IsEnabled(LogLevel.Debug))
                {
                    logger.LogDebug(ex, "Scheduler lock create race detected. Lock={LockName}", lockName);
                }
                return false;
            }
        }

        if (existing.ExpiresAtUtc > nowUtc && !string.Equals(existing.OwnerNodeId, _ownerId, StringComparison.Ordinal))
        {
            return false;
        }

        existing.OwnerNodeId = _ownerId;
        existing.AcquiredAtUtc = nowUtc;
        existing.ExpiresAtUtc = expiresAtUtc;

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug(ex, "Scheduler lock update race detected. Lock={LockName}", lockName);
            }
            return false;
        }
    }

    public async Task ReleaseAsync(string lockName, CancellationToken cancellationToken)
    {
        SchedulerLockLease? existing = await dbContext.SchedulerLockLeases
            .SingleOrDefaultAsync(x => x.LockName == lockName, cancellationToken);

        if (existing is null || !string.Equals(existing.OwnerNodeId, _ownerId, StringComparison.Ordinal))
        {
            return;
        }

        dbContext.SchedulerLockLeases.Remove(existing);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug(ex, "Scheduler lock release race detected. Lock={LockName}", lockName);
            }
        }
    }
}

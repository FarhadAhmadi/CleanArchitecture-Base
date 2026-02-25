using Application.Abstractions.Observability;
using Domain.Files;
using Domain.Logging;
using Domain.Notifications;
using Domain.Scheduler;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Monitoring;

public sealed class ObservabilityDashboardService(
    ApplicationReadDbContext dbContext,
    IOperationalMetricsService metricsService,
    IOrchestrationHealthService orchestrationHealthService) : IObservabilityDashboardService
{
    public async Task<ObservabilityDashboardSnapshot> GetSnapshotAsync(CancellationToken cancellationToken)
    {
        DateTime now = DateTime.UtcNow;
        DateTimeOffset nowOffset = DateTimeOffset.UtcNow;
        DateTime from24h = now.AddHours(-24);
        DateTime from7d = now.AddDays(-7);

        IQueryable<Domain.Logging.LogEvent> logQuery = dbContext.LogEvents.Where(x => !x.IsDeleted);
        IQueryable<Domain.Files.FileAsset> fileQuery = dbContext.FileAssets.Where(x => !x.IsDeleted);
        IQueryable<Domain.Modules.Notifications.NotificationMessage> notificationQuery = dbContext.NotificationMessages.Where(x => !x.IsArchived);
        IQueryable<Domain.Modules.Scheduler.ScheduledJob> jobQuery = dbContext.ScheduledJobs;

        int usersTotal = await dbContext.Users.CountAsync(cancellationToken);
        int usersWithProfile = await dbContext.UserProfiles.CountAsync(cancellationToken);
        int usersLocked = await dbContext.Users.CountAsync(
            x => x.LockoutEnd.HasValue && x.LockoutEnd.Value > nowOffset,
            cancellationToken);
        int usersNewLast7Days = await dbContext.Users.CountAsync(x => x.AuditCreatedAtUtc >= from7d, cancellationToken);

        int openAlerts = await dbContext.AlertIncidents.CountAsync(
            x => x.Status == "Open" || x.Status == "Queued" || x.Status == "Dispatching",
            cancellationToken);

        int auditEvents24h = await dbContext.AuditEntries.CountAsync(x => x.TimestampUtc >= from24h, cancellationToken);

        int totalLogs24h = await logQuery.CountAsync(x => x.TimestampUtc >= from24h, cancellationToken);
        int warningLogs24h = await logQuery.CountAsync(x => x.TimestampUtc >= from24h && x.Level == LogLevelType.Warning, cancellationToken);
        int errorLogs24h = await logQuery.CountAsync(
            x => x.TimestampUtc >= from24h && (x.Level == LogLevelType.Error || x.Level == LogLevelType.Critical),
            cancellationToken);
        int corruptedLogs = await logQuery.CountAsync(x => x.HasIntegrityIssue, cancellationToken);

        var logsByLevelRaw = await logQuery
            .Where(x => x.TimestampUtc >= from24h)
            .GroupBy(x => x.Level)
            .Select(g => new { g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToListAsync(cancellationToken);
        var logsByLevel = logsByLevelRaw
            .Select(x => new DashboardBucket(x.Key.ToString(), x.Count))
            .ToList();

        var topLogSourcesRaw = await logQuery
            .Where(x => x.TimestampUtc >= from24h)
            .GroupBy(x => x.SourceService)
            .Select(g => new { g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(8)
            .ToListAsync(cancellationToken);
        var topLogSources = topLogSourcesRaw
            .Select(x => new DashboardBucket(x.Key, x.Count))
            .ToList();

        int filesTotal = await fileQuery.CountAsync(cancellationToken);
        long totalFileBytes = await fileQuery.SumAsync(x => (long?)x.SizeBytes, cancellationToken) ?? 0L;
        int filesPendingStorage = await fileQuery.CountAsync(x => x.StorageStatus == FileStorageStatus.Pending, cancellationToken);
        int filesInfected = await fileQuery.CountAsync(x => x.IsInfected, cancellationToken);
        int filesEncrypted = await fileQuery.CountAsync(x => x.IsEncrypted, cancellationToken);

        var filesByModuleRaw = await fileQuery
            .GroupBy(x => x.Module)
            .Select(g => new { g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(8)
            .ToListAsync(cancellationToken);
        var filesByModule = filesByModuleRaw
            .Select(x => new DashboardBucket(x.Key, x.Count))
            .ToList();

        int notificationsTotal = await notificationQuery.CountAsync(cancellationToken);
        int notificationsPending = await notificationQuery.CountAsync(x => x.Status == NotificationStatus.Pending, cancellationToken);
        int notificationsScheduled = await notificationQuery.CountAsync(x => x.Status == NotificationStatus.Scheduled, cancellationToken);
        int notificationsFailed = await notificationQuery.CountAsync(x => x.Status == NotificationStatus.Failed, cancellationToken);
        int notificationsDelivered = await notificationQuery.CountAsync(x => x.Status == NotificationStatus.Delivered, cancellationToken);

        var notificationsByChannelRaw = await notificationQuery
            .GroupBy(x => x.Channel)
            .Select(g => new { g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToListAsync(cancellationToken);
        var notificationsByChannel = notificationsByChannelRaw
            .Select(x => new DashboardBucket(x.Key.ToString(), x.Count))
            .ToList();

        var notificationsByStatusRaw = await notificationQuery
            .GroupBy(x => x.Status)
            .Select(g => new { g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToListAsync(cancellationToken);
        var notificationsByStatus = notificationsByStatusRaw
            .Select(x => new DashboardBucket(x.Key.ToString(), x.Count))
            .ToList();

        int totalJobs = await jobQuery.CountAsync(cancellationToken);
        int activeJobs = await jobQuery.CountAsync(x => x.Status == JobStatus.Active, cancellationToken);
        int pausedJobs = await jobQuery.CountAsync(x => x.Status == JobStatus.Paused, cancellationToken);
        int quarantinedJobs = await jobQuery.CountAsync(x => x.IsQuarantined || x.Status == JobStatus.Quarantined, cancellationToken);
        int executions24h = await dbContext.JobExecutions.CountAsync(x => x.StartedAtUtc >= from24h, cancellationToken);
        int failedExecutions24h = await dbContext.JobExecutions.CountAsync(
            x => x.StartedAtUtc >= from24h && (x.Status == JobExecutionStatus.Failed || x.Status == JobExecutionStatus.DeadLettered),
            cancellationToken);

        OperationalMetricsSnapshot operationalMetrics = await metricsService.GetSnapshotAsync(cancellationToken);
        OrchestrationHealthSnapshot orchestrationHealth = await orchestrationHealthService.GetSnapshotAsync(cancellationToken);

        string loggingStatus = errorLogs24h > 0 || corruptedLogs > 0 ? "degraded" : "healthy";
        string notificationStatus = notificationsFailed > 0 ? "degraded" : "healthy";
        string schedulerStatus = failedExecutions24h > 0 || quarantinedJobs > 0 ? "degraded" : "healthy";
        string storageStatus = filesInfected > 0 || filesPendingStorage > 0 ? "degraded" : "healthy";

        string orchestrationStatus = string.Equals(orchestrationHealth.Status, "degraded", StringComparison.OrdinalIgnoreCase)
            ? "degraded"
            : "healthy";

        string globalStatus = (loggingStatus, notificationStatus, schedulerStatus, storageStatus, orchestrationStatus) switch
        {
            (_, _, _, _, "degraded") => "degraded",
            ("degraded", _, _, _, _) => "degraded",
            (_, "degraded", _, _, _) => "degraded",
            (_, _, "degraded", _, _) => "degraded",
            (_, _, _, "degraded", _) => "degraded",
            _ => "healthy"
        };

        DashboardHeadline headline = new(
            ActiveUsers: usersTotal,
            OpenAlerts: openAlerts,
            ErrorLogs24h: errorLogs24h,
            AuditEvents24h: auditEvents24h,
            PendingNotifications: notificationsPending + notificationsScheduled,
            ActiveJobs: activeJobs,
            TotalFiles: filesTotal);

        string integrityStatus = corruptedLogs switch
        {
            0 => "healthy",
            < 5 => "warning",
            _ => "critical"
        };

        DashboardIntegrity integrity = new(
            CheckedAtUtc: now,
            TotalRecords: await logQuery.CountAsync(cancellationToken),
            InvalidRecords: corruptedLogs,
            Status: integrityStatus);

        DashboardUsers users = new(
            Total: usersTotal,
            WithProfile: usersWithProfile,
            Locked: usersLocked,
            NewLast7Days: usersNewLast7Days);

        DashboardLogging logging = new(
            HealthStatus: loggingStatus,
            SchemaVersion: "1.0",
            TotalEvents24h: totalLogs24h,
            Warnings24h: warningLogs24h,
            Errors24h: errorLogs24h,
            CorruptedEvents: corruptedLogs,
            ByLevel: logsByLevel,
            TopSources: topLogSources);

        DashboardNotifications notifications = new(
            Total: notificationsTotal,
            Pending: notificationsPending,
            Scheduled: notificationsScheduled,
            Failed: notificationsFailed,
            Delivered: notificationsDelivered,
            ByChannel: notificationsByChannel,
            ByStatus: notificationsByStatus);

        DashboardScheduler scheduler = new(
            TotalJobs: totalJobs,
            ActiveJobs: activeJobs,
            PausedJobs: pausedJobs,
            QuarantinedJobs: quarantinedJobs,
            Executions24h: executions24h,
            FailedExecutions24h: failedExecutions24h);

        DashboardFiles files = new(
            Total: filesTotal,
            TotalSizeBytes: totalFileBytes,
            PendingStorage: filesPendingStorage,
            Infected: filesInfected,
            Encrypted: filesEncrypted,
            ByModule: filesByModule);

        DashboardServiceHealth[] services =
        [
            new("Logging", loggingStatus, $"errors24h={errorLogs24h} corrupted={corruptedLogs}"),
            new("Notification", notificationStatus, $"failed={notificationsFailed} pending={notificationsPending + notificationsScheduled}"),
            new("Scheduler", schedulerStatus, $"failedExec24h={failedExecutions24h} quarantined={quarantinedJobs}"),
            new("FileStorage", storageStatus, $"pending={filesPendingStorage} infected={filesInfected}"),
            new("Orchestration", orchestrationStatus, $"outboxFailed={orchestrationHealth.OutboxFailed} inboxFailed={orchestrationHealth.InboxFailed}"),
            new("IngestionQueue", operationalMetrics.IngestionQueueDepth > 0 ? "degraded" : "healthy", $"depth={operationalMetrics.IngestionQueueDepth} dropped={operationalMetrics.IngestionDropped}")
        ];

        return new ObservabilityDashboardSnapshot(
            GeneratedAtUtc: now,
            Status: globalStatus,
            Headline: headline,
            Integrity: integrity,
            Users: users,
            Logging: logging,
            Notifications: notifications,
            Scheduler: scheduler,
            Files: files,
            Services: services);
    }
}

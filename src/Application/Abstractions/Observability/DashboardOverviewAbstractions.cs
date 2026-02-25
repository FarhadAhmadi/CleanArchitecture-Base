namespace Application.Abstractions.Observability;

public interface IObservabilityDashboardService
{
    Task<ObservabilityDashboardSnapshot> GetSnapshotAsync(CancellationToken cancellationToken);
}

public sealed record ObservabilityDashboardSnapshot(
    DateTime GeneratedAtUtc,
    string Status,
    DashboardHeadline Headline,
    DashboardIntegrity Integrity,
    DashboardUsers Users,
    DashboardLogging Logging,
    DashboardNotifications Notifications,
    DashboardScheduler Scheduler,
    DashboardFiles Files,
    IReadOnlyList<DashboardServiceHealth> Services);

public sealed record DashboardHeadline(
    int ActiveUsers,
    int OpenAlerts,
    int ErrorLogs24h,
    int AuditEvents24h,
    int PendingNotifications,
    int ActiveJobs,
    int TotalFiles);

public sealed record DashboardIntegrity(
    DateTime CheckedAtUtc,
    int TotalRecords,
    int InvalidRecords,
    string Status);

public sealed record DashboardUsers(
    int Total,
    int WithProfile,
    int Locked,
    int NewLast7Days);

public sealed record DashboardLogging(
    string HealthStatus,
    string SchemaVersion,
    int TotalEvents24h,
    int Warnings24h,
    int Errors24h,
    int CorruptedEvents,
    IReadOnlyList<DashboardBucket> ByLevel,
    IReadOnlyList<DashboardBucket> TopSources);

public sealed record DashboardNotifications(
    int Total,
    int Pending,
    int Scheduled,
    int Failed,
    int Delivered,
    IReadOnlyList<DashboardBucket> ByChannel,
    IReadOnlyList<DashboardBucket> ByStatus);

public sealed record DashboardScheduler(
    int TotalJobs,
    int ActiveJobs,
    int PausedJobs,
    int QuarantinedJobs,
    int Executions24h,
    int FailedExecutions24h);

public sealed record DashboardFiles(
    int Total,
    long TotalSizeBytes,
    int PendingStorage,
    int Infected,
    int Encrypted,
    IReadOnlyList<DashboardBucket> ByModule);

public sealed record DashboardBucket(string Key, int Count);

public sealed record DashboardServiceHealth(
    string Name,
    string Status,
    string Summary);

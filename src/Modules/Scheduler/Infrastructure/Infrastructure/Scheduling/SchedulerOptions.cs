namespace Infrastructure.Scheduler;

public sealed class SchedulerOptions
{
    public const string SectionName = "Scheduler";
    public bool SeedDefaults { get; set; } = true;
    public int PollingIntervalSeconds { get; set; } = 15;
    public int MaxDueJobsPerIteration { get; set; } = 100;
    public int LockLeaseSeconds { get; set; } = 120;
    public int MisfireGraceSeconds { get; set; } = 30;
    public int DefaultQuarantineMinutes { get; set; } = 60;
    public string? NodeId { get; set; }
}

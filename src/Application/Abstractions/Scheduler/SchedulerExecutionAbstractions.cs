using Domain.Modules.Scheduler;
using Domain.Scheduler;

namespace Application.Abstractions.Scheduler;

public interface IScheduledJobHandler
{
    JobType JobType { get; }
    Task ExecuteAsync(ScheduledJob job, CancellationToken cancellationToken);
}

public sealed record SchedulerExecutionRequest(
    ScheduledJob Job,
    string TriggeredBy,
    string NodeId,
    int Attempt,
    int MaxAttempts,
    DateTime ScheduledAtUtc,
    bool IsReplay);

public sealed record SchedulerExecutionResult(
    JobExecutionStatus Status,
    int DurationMs,
    string? Error,
    bool TimedOut,
    bool Canceled);

public interface ISchedulerExecutionService
{
    Task<SchedulerExecutionResult> ExecuteAsync(SchedulerExecutionRequest request, CancellationToken cancellationToken);
}

public sealed record SchedulerPayloadValidationResult(
    bool IsValid,
    string? NormalizedPayloadJson,
    string? Error);

public interface ISchedulerPayloadValidator
{
    SchedulerPayloadValidationResult Validate(JobType type, string? payloadJson);
}

public sealed record SchedulerRetryPolicy(
    int MaxAttempts,
    int BaseDelaySeconds,
    int MaxDelaySeconds);

public interface ISchedulerRetryPolicyProvider
{
    SchedulerRetryPolicy GetPolicy(JobType jobType, ScheduledJob job);
}

public interface ISchedulerDistributedLockProvider
{
    Task<bool> TryAcquireAsync(string lockName, TimeSpan leaseDuration, CancellationToken cancellationToken);
    Task ReleaseAsync(string lockName, CancellationToken cancellationToken);
}

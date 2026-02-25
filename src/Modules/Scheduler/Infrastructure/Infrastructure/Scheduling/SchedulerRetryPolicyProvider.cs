using Application.Abstractions.Scheduler;
using Domain.Modules.Scheduler;
using Domain.Scheduler;

namespace Infrastructure.Scheduler;

internal sealed class SchedulerRetryPolicyProvider : ISchedulerRetryPolicyProvider
{
    public SchedulerRetryPolicy GetPolicy(JobType jobType, ScheduledJob job)
    {
        return jobType switch
        {
            JobType.CleanupOldSchedulerExecutions => new SchedulerRetryPolicy(
                Math.Clamp(job.MaxRetryAttempts, 1, 10),
                Math.Clamp(job.RetryBackoffSeconds, 1, 3600),
                3600),
            JobType.NotificationDispatchProbe => new SchedulerRetryPolicy(
                Math.Clamp(job.MaxRetryAttempts, 1, 10),
                Math.Clamp(job.RetryBackoffSeconds, 1, 900),
                1800),
            _ => new SchedulerRetryPolicy(
                Math.Clamp(job.MaxRetryAttempts, 1, 10),
                Math.Clamp(job.RetryBackoffSeconds, 1, 3600),
                3600)
        };
    }
}

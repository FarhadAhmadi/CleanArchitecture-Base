using Cronos;
using Domain.Modules.Scheduler;
using Domain.Scheduler;

namespace Application.Scheduler;

public static class SchedulerCalculations
{
    public static bool TryValidateSchedule(
        ScheduleType type,
        string? cronExpression,
        int? intervalSeconds,
        DateTime? oneTimeAtUtc,
        out string? error)
    {
        error = null;
        if (type == ScheduleType.Cron)
        {
            if (string.IsNullOrWhiteSpace(cronExpression))
            {
                error = "CronExpression is required for cron schedules.";
                return false;
            }

            if (!TryParseCron(cronExpression, out _))
            {
                error = "CronExpression is not valid.";
                return false;
            }
        }

        if (type == ScheduleType.Interval && (!intervalSeconds.HasValue || intervalSeconds.Value <= 0))
        {
            error = "IntervalSeconds must be greater than zero for interval schedules.";
            return false;
        }

        if (type == ScheduleType.OneTime && !oneTimeAtUtc.HasValue)
        {
            error = "OneTimeAtUtc is required for one-time schedules.";
            return false;
        }

        return true;
    }

    public static DateTime? ComputeNextRunUtc(
        ScheduleType type,
        string? cronExpression,
        int? intervalSeconds,
        DateTime? oneTimeAtUtc,
        DateTime fromUtc)
    {
        return type switch
        {
            ScheduleType.OneTime => oneTimeAtUtc,
            ScheduleType.Interval => fromUtc.AddSeconds(Math.Max(1, intervalSeconds ?? 60)),
            ScheduleType.Cron => TryParseCron(cronExpression, out CronExpression? cron) && cron is not null
                ? cron.GetNextOccurrence(fromUtc, TimeZoneInfo.Utc, false)
                : null,
            _ => fromUtc.AddMinutes(1)
        };
    }

    public static int ComputeBackoffSeconds(
        int attempt,
        int baseDelaySeconds,
        int maxDelaySeconds)
    {
        int safeAttempt = Math.Max(1, attempt);
        double exponential = baseDelaySeconds * Math.Pow(2, safeAttempt - 1);
        int bounded = (int)Math.Min(maxDelaySeconds, Math.Round(exponential));
        return Math.Max(1, bounded);
    }

    public static bool IsJobRunnable(
        ScheduledJob job,
        JobSchedule schedule,
        DateTime nowUtc)
    {
        if (job.Status is JobStatus.Inactive or JobStatus.Paused or JobStatus.Quarantined)
        {
            return false;
        }

        if (!schedule.IsEnabled)
        {
            return false;
        }

        if (schedule.StartAtUtc.HasValue && schedule.StartAtUtc.Value > nowUtc)
        {
            return false;
        }

        if (schedule.EndAtUtc.HasValue && schedule.EndAtUtc.Value < nowUtc)
        {
            return false;
        }

        return !schedule.NextRunAtUtc.HasValue || schedule.NextRunAtUtc.Value <= nowUtc;
    }

    private static bool TryParseCron(string? expression, out CronExpression? cron)
    {
        cron = null;
        if (string.IsNullOrWhiteSpace(expression))
        {
            return false;
        }

        string normalized = expression.Trim();
        try
        {
            cron = CronExpression.Parse(normalized, CronFormat.Standard);
            return true;
        }
        catch (CronFormatException)
        {
            try
            {
                cron = CronExpression.Parse(normalized, CronFormat.IncludeSeconds);
                return true;
            }
            catch (CronFormatException)
            {
                return false;
            }
        }
    }
}

using System.Diagnostics.Metrics;
using Domain.Scheduler;

namespace Infrastructure.Scheduler;

public sealed class SchedulerMetrics
{
    public const string MeterName = "CleanArchitecture.Scheduler";

    private readonly Counter<long> _successCounter;
    private readonly Counter<long> _failureCounter;
    private readonly Counter<long> _timeoutCounter;
    private readonly Counter<long> _deadLetterCounter;
    private readonly Histogram<double> _durationMsHistogram;
    private readonly Histogram<double> _queueLagMsHistogram;

    public SchedulerMetrics(IMeterFactory meterFactory)
    {
        #pragma warning disable CA2000
        Meter meter = meterFactory.Create(MeterName);
        #pragma warning restore CA2000
        _successCounter = meter.CreateCounter<long>("scheduler.success.count");
        _failureCounter = meter.CreateCounter<long>("scheduler.failure.count");
        _timeoutCounter = meter.CreateCounter<long>("scheduler.timeout.count");
        _deadLetterCounter = meter.CreateCounter<long>("scheduler.deadletter.count");
        _durationMsHistogram = meter.CreateHistogram<double>("scheduler.job.duration.ms");
        _queueLagMsHistogram = meter.CreateHistogram<double>("scheduler.queue.lag.ms");
    }

    public void RecordExecution(
        string jobType,
        JobExecutionStatus status,
        int durationMs,
        double queueLagMs)
    {
        KeyValuePair<string, object?>[] tags = [new("job_type", jobType), new("status", status.ToString())];
        _durationMsHistogram.Record(Math.Max(0, durationMs), tags);
        _queueLagMsHistogram.Record(Math.Max(0, queueLagMs), tags);

        switch (status)
        {
            case JobExecutionStatus.Succeeded:
                _successCounter.Add(1, tags);
                break;
            case JobExecutionStatus.TimedOut:
                _timeoutCounter.Add(1, tags);
                _failureCounter.Add(1, tags);
                break;
            case JobExecutionStatus.DeadLettered:
                _deadLetterCounter.Add(1, tags);
                _failureCounter.Add(1, tags);
                break;
            case JobExecutionStatus.Failed:
            case JobExecutionStatus.Canceled:
                _failureCounter.Add(1, tags);
                break;
        }
    }
}

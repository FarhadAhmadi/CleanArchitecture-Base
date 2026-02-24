using System.Diagnostics.Metrics;

namespace Infrastructure.Files;

public sealed class FileStorageMetrics
{
    public const string MeterName = "CleanArchitecture.Files";

    private readonly Counter<long> _operationCounter;
    private readonly Counter<long> _objectNotFoundCounter;
    private readonly Histogram<double> _latencyMs;

    public FileStorageMetrics(IMeterFactory meterFactory)
    {
        #pragma warning disable CA2000
        Meter meter = meterFactory.Create(MeterName);
        #pragma warning restore CA2000
        _operationCounter = meter.CreateCounter<long>("files.storage.operations.count");
        _objectNotFoundCounter = meter.CreateCounter<long>("files.storage.object_not_found.count");
        _latencyMs = meter.CreateHistogram<double>("files.storage.operation.duration.ms");
    }

    public void RecordOperation(string operation, string outcome, double elapsedMs)
    {
        KeyValuePair<string, object?>[] tags =
        [
            new("operation", operation),
            new("outcome", outcome)
        ];

        _operationCounter.Add(1, tags);
        _latencyMs.Record(Math.Max(0, elapsedMs), tags);
    }

    public void RecordObjectNotFound(string operation)
    {
        _objectNotFoundCounter.Add(1, new KeyValuePair<string, object?>("operation", operation));
    }
}

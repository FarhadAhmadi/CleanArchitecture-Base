using System.Threading.Channels;

namespace Infrastructure.Logging;

internal sealed class AlertDispatchQueue : IAlertDispatchQueue
{
    private readonly Channel<Guid> _channel = Channel.CreateUnbounded<Guid>();
    private int _count;

    public int ApproximateCount => _count;

    public bool TryEnqueue(Guid incidentId)
    {
        bool ok = _channel.Writer.TryWrite(incidentId);
        if (ok)
        {
            Interlocked.Increment(ref _count);
        }

        return ok;
    }

    public async ValueTask<Guid> DequeueAsync(CancellationToken cancellationToken)
    {
        Guid id = await _channel.Reader.ReadAsync(cancellationToken);
        Interlocked.Decrement(ref _count);
        return id;
    }
}

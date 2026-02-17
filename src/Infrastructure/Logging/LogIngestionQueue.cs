using System.Threading.Channels;
using Domain.Logging;

namespace Infrastructure.Logging;

internal sealed class LogIngestionQueue : ILogIngestionQueue
{
    private readonly Channel<LogEvent> _channel;
    private int _count;
    private long _dropped;

    public LogIngestionQueue(LoggingOptions options)
    {
        _channel = Channel.CreateBounded<LogEvent>(new BoundedChannelOptions(options.QueueCapacity)
        {
            SingleReader = false,
            SingleWriter = false,
            FullMode = BoundedChannelFullMode.DropWrite
        });
    }

    public int ApproximateCount => _count;
    public long DroppedCount => Interlocked.Read(ref _dropped);

    public bool TryEnqueue(LogEvent item)
    {
        bool written = _channel.Writer.TryWrite(item);
        if (written)
        {
            Interlocked.Increment(ref _count);
        }
        else
        {
            Interlocked.Increment(ref _dropped);
        }

        return written;
    }

    public async ValueTask<LogEvent> DequeueAsync(CancellationToken cancellationToken)
    {
        LogEvent item = await _channel.Reader.ReadAsync(cancellationToken);
        Interlocked.Decrement(ref _count);
        return item;
    }
}

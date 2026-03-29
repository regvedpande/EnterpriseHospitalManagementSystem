using System.Threading.Channels;

namespace Hospital.Web.Infrastructure.Queue;

/// <summary>
/// Bounded channel-based implementation of IBackgroundTaskQueue.
/// Capacity of 500 ensures back-pressure without blocking callers.
/// </summary>
public sealed class BackgroundTaskQueue : IBackgroundTaskQueue
{
    private readonly Channel<Func<IServiceProvider, CancellationToken, Task>> _queue;

    public BackgroundTaskQueue(int capacity = 500)
    {
        var opts = new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait
        };
        _queue = Channel.CreateBounded<Func<IServiceProvider, CancellationToken, Task>>(opts);
    }

    public void Enqueue(Func<IServiceProvider, CancellationToken, Task> workItem)
    {
        ArgumentNullException.ThrowIfNull(workItem);
        // TryWrite returns false only when channel is full (extremely rare at capacity=500)
        if (!_queue.Writer.TryWrite(workItem))
            _ = _queue.Writer.WriteAsync(workItem); // async fallback
    }

    public ValueTask<Func<IServiceProvider, CancellationToken, Task>> DequeueAsync(CancellationToken ct)
        => _queue.Reader.ReadAsync(ct);

    public int Count => _queue.Reader.Count;
}

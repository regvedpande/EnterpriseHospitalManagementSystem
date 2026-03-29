namespace Hospital.Web.Infrastructure.Queue;

/// <summary>
/// In-process background task queue backed by System.Threading.Channels.
/// Use for fire-and-forget work: email, SMS, event publishing.
/// </summary>
public interface IBackgroundTaskQueue
{
    /// <summary>Enqueue an async work item to be executed in the background.</summary>
    void Enqueue(Func<IServiceProvider, CancellationToken, Task> workItem);

    /// <summary>Dequeue the next work item (blocks until one is available or cancelled).</summary>
    ValueTask<Func<IServiceProvider, CancellationToken, Task>> DequeueAsync(CancellationToken ct);

    /// <summary>Current number of pending items in the queue.</summary>
    int Count { get; }
}

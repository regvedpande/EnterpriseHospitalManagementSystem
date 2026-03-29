using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Hospital.Web.Infrastructure.Queue;

/// <summary>
/// BackgroundService that continuously drains the IBackgroundTaskQueue.
/// Work items receive a fresh DI scope so they can resolve Scoped services.
/// </summary>
public sealed class QueuedHostedService : BackgroundService
{
    private readonly IBackgroundTaskQueue _queue;
    private readonly IServiceProvider     _root;
    private readonly ILogger<QueuedHostedService> _log;

    public QueuedHostedService(
        IBackgroundTaskQueue queue,
        IServiceProvider root,
        ILogger<QueuedHostedService> log)
    {
        _queue = queue;
        _root  = root;
        _log   = log;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _log.LogInformation("[Queue] Background task processor started. Queue depth: {Depth}", _queue.Count);

        while (!stoppingToken.IsCancellationRequested)
        {
            Func<IServiceProvider, CancellationToken, Task> workItem;
            try
            {
                workItem = await _queue.DequeueAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            // Create a fresh DI scope per work item so Scoped deps are properly disposed.
            await using var scope = _root.CreateAsyncScope();
            try
            {
                await workItem(scope.ServiceProvider, stoppingToken);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "[Queue] Unhandled exception processing background work item");
            }
        }

        _log.LogInformation("[Queue] Background task processor stopped.");
    }
}

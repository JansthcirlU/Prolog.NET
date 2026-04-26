using System.Collections.Concurrent;

namespace Prolog.NET.Threading;

/// <summary>
/// Simple worker to allow running jobs on a separate thread.
/// Assumes that any enqueued jobs succeeds without failure.
/// Dispose finishes remaining work.
/// </summary>
/// <remarks>This type is not thread safe.</remarks>
public class ThreadWorker : IDisposable
{
    private readonly Thread _workerThread;
    private readonly BlockingCollection<Action> _workerQueue;

    public ThreadWorker()
    {
        _workerQueue = [];
        _workerThread = new(Run)
        {
            IsBackground = true
        };
        _workerThread.Start();
    }

    private void Run()
    {
        foreach (Action job in _workerQueue.GetConsumingEnumerable())
        {
            job.Invoke();
        }
    }

    public void AddJob(Action job)
    {
        _workerQueue.Add(job);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _workerQueue.CompleteAdding();
        _workerThread.Join();
        _workerQueue.Dispose();
    }
}

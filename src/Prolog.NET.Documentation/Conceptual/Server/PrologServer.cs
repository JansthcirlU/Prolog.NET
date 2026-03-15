using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Prolog.NET.Documentation.Conceptual.Swipl;
using Prolog.NET.Documentation.Conceptual.Worker;

namespace Prolog.NET.Documentation.Conceptual.Server;

/// <summary>
/// Manages PrologWorker instances and coordinates query execution across loaded files.
/// </summary>
internal sealed class PrologServer
{
    private readonly ConcurrentDictionary<string, ConcurrentBag<PrologWorker>> _fileWorkers;
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _creationLocks;
    private static readonly Lazy<PrologServer> _instance = new(() => new());

    internal static PrologServer Instance => _instance.Value;

    private PrologServer()
    {
        _fileWorkers = [];
        _creationLocks = [];
    }

    internal async IAsyncEnumerable<PrologWorkerResponse> QueryFileAsync(string fileName, string goal, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        PrologWorker worker = await LoadFileWorkerAsync(fileName, cancellationToken);
        await foreach (PrologWorkerResponse workerResponse in worker.QueryFileAsync(goal, cancellationToken))
        {
            yield return workerResponse;
        }
    }

    private async Task<PrologWorker> LoadFileWorkerAsync(string fileName, CancellationToken cancellationToken)
    {
        ConcurrentBag<PrologWorker> workers = _fileWorkers.GetOrAdd(fileName, []);

        if (workers.TryPeek(out PrologWorker? existing))
        {
            return existing;
        }

        SemaphoreSlim creationLock = _creationLocks.GetOrAdd(fileName, _ => new SemaphoreSlim(1, 1));
        await creationLock.WaitAsync(cancellationToken);
        try
        {
            // Re-check after acquiring the lock — another thread may have created the worker while we were waiting
            if (workers.TryPeek(out existing))
            {
                return existing;
            }

            PrologActor actor = PrologActor.Create(fileName);
            PrologWorker worker = await PrologWorker.StartNewAsync(actor, cancellationToken);
            workers.Add(worker);
            return worker;
        }
        finally
        {
            creationLock.Release();
        }
    }
}

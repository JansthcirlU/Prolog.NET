using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Prolog.NET.Documentation.Conceptual.Swipl;
using Prolog.NET.Documentation.Conceptual.Worker;

namespace Prolog.NET.Documentation.Conceptual.Server;

internal sealed class WorkerRegistry
{
    private readonly ConcurrentDictionary<string, ConcurrentBag<PrologWorker>> _fileWorkers;
    private static readonly Lazy<WorkerRegistry> _instance = new(() => new());

    internal static WorkerRegistry Instance => _instance.Value;

    private WorkerRegistry()
    {
        _fileWorkers = [];
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
        SwiPrologWrapper wrapper = SwiPrologWrapper.Create();
        PrologWorker worker = await PrologWorker.StartNewAsync(wrapper, cancellationToken);
        workers.Add(worker);
        return worker;
    }
}

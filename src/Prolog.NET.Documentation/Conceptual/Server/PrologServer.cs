using System.Runtime.CompilerServices;
using Prolog.NET.Documentation.Conceptual.Worker;

namespace Prolog.NET.Documentation.Conceptual.Server;

/// <summary>
/// Describes the API surface of the PrologServer
/// </summary>
internal sealed class PrologServer
{
    private readonly WorkerRegistry _registry;
    private static readonly Lazy<PrologServer> _instance = new(() => new(WorkerRegistry.Instance));

    internal static PrologServer Instance => _instance.Value;

    private PrologServer(WorkerRegistry registry)
    {
        _registry = registry;
    }

    internal async IAsyncEnumerable<PrologWorkerResponse> QueryFileAsync(string fileName, string goal, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (PrologWorkerResponse workerResponse in _registry.QueryFileAsync(fileName, goal, cancellationToken))
        {
            yield return workerResponse;
        }
    }
}
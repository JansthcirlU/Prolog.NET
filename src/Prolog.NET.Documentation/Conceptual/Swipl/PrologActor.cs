using System.Collections.Concurrent;

namespace Prolog.NET.Documentation.Conceptual.Swipl;

internal sealed class PrologActor
{
    private readonly ConcurrentDictionary<Guid, PrologEngine> _engines;
    private readonly string _fileName;

    private PrologActor(string fileName)
    {
        _engines = [];
        _fileName = fileName;
    }

    internal static PrologActor Create(string fileName)
        => new(fileName);

    internal Task<PrologEngine?> QueryAsync(string goal, CancellationToken cancellationToken)
    {
        try
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromResult<PrologEngine?>(null);
            }

            Guid id = Guid.NewGuid();
            PrologEngine engine = new(id, _fileName, goal);
            if (!_engines.TryAdd(id, engine))
            {
                return Task.FromException<PrologEngine?>(new InvalidOperationException($"Tried to create PrologEngine with duplicate ID: {id}"));
            }
            return Task.FromResult<PrologEngine?>(engine);
        }
        catch (OperationCanceledException)
        {
            return Task.FromResult<PrologEngine?>(null);
        }
        catch (Exception ex)
        {
            return Task.FromException<PrologEngine?>(ex);
        }
    }

    internal Task<bool> DestroyEngine(PrologEngine engine)
        => Task.FromResult(_engines.TryRemove(new(engine.Id, engine)));
}

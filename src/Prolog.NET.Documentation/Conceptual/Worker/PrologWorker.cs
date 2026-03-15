using System.Runtime.CompilerServices;
using Prolog.NET.Documentation.Conceptual.Swipl;

namespace Prolog.NET.Documentation.Conceptual.Worker;

internal sealed class PrologWorker
{
    private readonly PrologActor _swipl;

    private PrologWorker(PrologActor swipl)
    {
        _swipl = swipl;
    }

    internal static Task<PrologWorker> StartNewAsync(PrologActor swipl, CancellationToken cancellationToken)
        => cancellationToken.IsCancellationRequested
            ? Task.FromCanceled<PrologWorker>(cancellationToken)
            : Task.FromResult<PrologWorker>(new(swipl));

    internal async IAsyncEnumerable<PrologWorkerResponse> QueryFileAsync(string goal, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        PrologEngine? engine = await _swipl.QueryAsync(goal, cancellationToken);
        if (engine is null)
        {
            yield break;
        }
        await engine.InitialiseAsync();

        try
        {
            do
            {
                PrologEngineResponse? next;
                Exception? exception;
                try
                {
                    next = await engine.GetNextResponseAsync(cancellationToken);
                    exception = null;
                }
                catch (Exception ex)
                {
                    exception = ex;
                    next = null;
                }

                if (exception is not null && next is null)
                {
                    yield return PrologWorkerResponse.FromException(new(null, exception));
                    yield break;
                }

                if (next is not null)
                {
                    yield return PrologWorkerResponse.FromEngine(next);

                    if (next is PrologEngineResponse.FinalSolutionResponse)
                    {
                        break;
                    }
                    if (next is PrologEngineResponse.ExceptionResponse)
                    {
                        break;
                    }
                    if (next is PrologEngineResponse.QueryHaltedResponse)
                    {
                        break;
                    }
                }
            } while (true);
        }
        finally
        {
            await _swipl.DestroyEngine(engine);
        }
    }
}

using System.Runtime.CompilerServices;
using Prolog.NET.Documentation.Conceptual.Swipl;

namespace Prolog.NET.Documentation.Conceptual.Worker;

internal sealed class PrologWorker
{
    private readonly SwiPrologWrapper _swipl;

    private PrologWorker(SwiPrologWrapper swipl)
    {
        _swipl = swipl;
    }

    internal static Task<PrologWorker> StartNewAsync(SwiPrologWrapper swipl, CancellationToken cancellationToken)
        => cancellationToken.IsCancellationRequested
            ? Task.FromResult<PrologWorker>(new(swipl))
            : Task.FromCanceled<PrologWorker>(cancellationToken);

    internal async IAsyncEnumerable<PrologWorkerResponse> QueryFileAsync(string goal, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        PrologEngine? engine = await _swipl.QueryAsync(goal, cancellationToken);
        if (engine is null)
        {
            yield break;
        }
        await engine.InitialiseAsync();

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
                yield return PrologWorkerResponse.FromException(exception);
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
            }
        } while (true);

        await _swipl.DestroyEngine(engine);
        yield break;
    }
}

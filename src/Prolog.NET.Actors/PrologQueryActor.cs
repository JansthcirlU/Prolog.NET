using Proto;
using Prolog.NET.Swipl;

namespace Prolog.NET.Actors;

/// <summary>
/// A short-lived actor wrapping a single open <see cref="PrologQuery"/>.
/// Spawned by <see cref="PrologWorkerActor"/> for each streaming query; stopped when
/// the query is exhausted, encounters an error, or receives a <see cref="CloseQueryMessage"/>.
/// </summary>
/// <remarks>
/// This actor is NOT registered with DI — it is instantiated directly via a
/// <c>Props.FromProducer</c> closure that captures the already-opened <see cref="PrologQuery"/>.
/// </remarks>
public sealed class PrologQueryActor : IActor
{
    private PrologQuery? _query;

    public PrologQueryActor(PrologQuery query) => _query = query;

    public Task ReceiveAsync(IContext context)
    {
        switch (context.Message)
        {
            case NextSolutionMessage:
                HandleNextSolution(context);
                break;
            case CloseQueryMessage:
                DisposeQuery();
                context.Stop(context.Self);
                break;
            case Stopping:
                DisposeQuery();
                break;
        }

        return Task.CompletedTask;
    }

    private void HandleNextSolution(IContext context)
    {
        if (_query == null)
        {
            context.Respond(new NextSolutionResponse { NoMore = new NoMoreSolutionsResult() });
            return;
        }

        try
        {
            if (_query.Next())
            {
                Dictionary<string, string> vars = BuildVars(_query.Current!);

                if (_query.IsLastSolution)
                {
                    DisposeQuery();
                    FinalSolutionResult result = new();
                    result.Variables.Add(vars);
                    context.Respond(new NextSolutionResponse { FinalSolution = result });
                    context.Stop(context.Self);
                }
                else
                {
                    SolutionResult result = new();
                    result.Variables.Add(vars);
                    context.Respond(new NextSolutionResponse { Solution = result });
                }
            }
            else
            {
                DisposeQuery();
                context.Respond(new NextSolutionResponse { NoMore = new NoMoreSolutionsResult() });
                context.Stop(context.Self);
            }
        }
        catch (PrologException ex)
        {
            DisposeQuery();
            context.Respond(new NextSolutionResponse
            {
                Failed = new QueryFailedResult { Error = ex.PrologMessage ?? ex.Message }
            });
            context.Stop(context.Self);
        }
    }

    private void DisposeQuery()
    {
        _query?.Dispose();
        _query = null;
    }

    private static Dictionary<string, string> BuildVars(PrologSolution solution)
        => solution.VariableNames.ToDictionary(name => name, name => solution[name].ToString());
}

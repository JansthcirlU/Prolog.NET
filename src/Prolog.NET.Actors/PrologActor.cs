using Proto;
using Prolog.NET.Swipl;

namespace Prolog.NET.Actors;

/// <summary>
/// A Proto.Actor actor that fronts a <see cref="PrologEngine"/> instance with
/// request-reply message semantics using Protocol Buffer message types.
/// </summary>
/// <remarks>
/// Supported inbound messages: <see cref="LoadFileMessage"/>, <see cref="CallMessage"/>,
/// <see cref="QueryMessage"/>, <see cref="OpenQueryMessage"/>, <see cref="NextSolutionMessage"/>,
/// <see cref="CloseQueryMessage"/>. Unknown messages are silently ignored.
///
/// Exceptions from the Prolog engine are caught per-handler and returned as error responses
/// rather than propagating — this prevents Proto.Actor's supervisor restart strategy from
/// firing and ensures callers always receive a reply.
/// </remarks>
public class PrologActor(PrologEngine engine) : IActor
{
    private readonly Dictionary<Guid, PrologQuery> _openQueries = [];

    public Task ReceiveAsync(IContext context)
    {
        switch (context.Message)
        {
            case LoadFileMessage msg:
                HandleLoadFile(context, msg);
                break;
            case CallMessage msg:
                HandleCall(context, msg);
                break;
            case QueryMessage msg:
                HandleQuery(context, msg);
                break;
            case OpenQueryMessage msg:
                HandleOpenQuery(context, msg);
                break;
            case NextSolutionMessage msg:
                HandleNextSolution(context, msg);
                break;
            case CloseQueryMessage msg:
                HandleCloseQuery(msg);
                break;
            case Stopping:
                CloseAllOpenQueries();
                break;
        }

        return Task.CompletedTask;
    }

    private void HandleLoadFile(IContext context, LoadFileMessage msg)
    {
        try
        {
            engine.LoadFile(msg.Path);
            context.Respond(new CallResult { Success = true });
        }
        catch (PrologException ex)
        {
            context.Respond(new CallResult { Success = false, ErrorMessage = ex.PrologMessage ?? ex.Message });
        }
    }

    private void HandleCall(IContext context, CallMessage msg)
    {
        try
        {
            bool ok = engine.Call(msg.Goal);
            context.Respond(new CallResult { Success = ok });
        }
        catch (PrologException ex)
        {
            context.Respond(new CallResult { Success = false, ErrorMessage = ex.PrologMessage ?? ex.Message });
        }
    }

    private void HandleQuery(IContext context, QueryMessage msg)
    {
        try
        {
            var result = new QueryResult();
            using PrologQuery query = engine.OpenQuery(msg.Goal);
            foreach (PrologSolution solution in query.Solutions)
            {
                var row = new SolutionRow();
                foreach (string name in solution.VariableNames)
                    row.Variables[name] = solution[name].ToString();
                result.Solutions.Add(row);
            }

            context.Respond(result);
        }
        catch (PrologException ex)
        {
            context.Respond(new QueryResult { ErrorMessage = ex.PrologMessage ?? ex.Message });
        }
    }

    private void HandleOpenQuery(IContext context, OpenQueryMessage msg)
    {
        try
        {
            PrologQuery query = engine.OpenQuery(msg.Goal);
            Guid id = Guid.NewGuid();
            _openQueries[id] = query;
            context.Respond(new OpenQueryResponse
            {
                Opened = new QueryOpenedResult { QueryId = id.ToString() }
            });
        }
        catch (PrologException ex)
        {
            context.Respond(new OpenQueryResponse
            {
                Failed = new OpenQueryFailedResult { Error = ex.PrologMessage ?? ex.Message }
            });
        }
    }

    private void HandleNextSolution(IContext context, NextSolutionMessage msg)
    {
        if (!Guid.TryParse(msg.QueryId, out Guid id) || !_openQueries.TryGetValue(id, out PrologQuery? query))
        {
            context.Respond(new NextSolutionResponse { NoMore = new NoMoreSolutionsResult() });
            return;
        }

        try
        {
            if (query.Next())
            {
                Dictionary<string, string> vars = BuildVars(query.Current!);

                if (query.IsLastSolution)
                {
                    _ = _openQueries.Remove(id);
                    query.Dispose();
                    var result = new FinalSolutionResult();
                    result.Variables.Add(vars);
                    context.Respond(new NextSolutionResponse { FinalSolution = result });
                }
                else
                {
                    var result = new SolutionResult();
                    result.Variables.Add(vars);
                    context.Respond(new NextSolutionResponse { Solution = result });
                }
            }
            else
            {
                _ = _openQueries.Remove(id);
                query.Dispose();
                context.Respond(new NextSolutionResponse { NoMore = new NoMoreSolutionsResult() });
            }
        }
        catch (PrologException ex)
        {
            _ = _openQueries.Remove(id);
            query.Dispose();
            context.Respond(new NextSolutionResponse
            {
                Failed = new QueryFailedResult { Error = ex.PrologMessage ?? ex.Message }
            });
        }
    }

    private void HandleCloseQuery(CloseQueryMessage msg)
    {
        if (Guid.TryParse(msg.QueryId, out Guid id) && _openQueries.Remove(id, out PrologQuery? query))
        {
            query.Dispose();
        }
    }

    private void CloseAllOpenQueries()
    {
        foreach (PrologQuery query in _openQueries.Values)
        {
            query.Dispose();
        }

        _openQueries.Clear();
    }

    private static Dictionary<string, string> BuildVars(PrologSolution solution)
        => solution.VariableNames.ToDictionary(name => name, name => solution[name].ToString());
}

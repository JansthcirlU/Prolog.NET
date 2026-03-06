using Proto;
using Prolog.NET.Swipl;

namespace Prolog.NET.Actors;

/// <summary>
/// A Proto.Actor actor that fronts a <see cref="PrologEngine"/> instance with
/// request-reply message semantics.
/// </summary>
/// <remarks>
/// <para>
/// Supported inbound messages: <see cref="LoadFileMessage"/>, <see cref="CallMessage"/>,
/// <see cref="QueryMessage"/>, <see cref="OpenQueryMessage"/>, <see cref="NextSolutionMessage"/>,
/// <see cref="CloseQueryMessage"/>. Unknown messages are silently ignored.
/// </para>
/// <para>
/// Exceptions from the Prolog engine are caught per-handler and returned as error responses
/// (<see cref="CallResult.ErrorMessage"/> / <see cref="QueryResult.ErrorMessage"/> /
/// <see cref="QueryErrorMessage"/>) rather than propagating — this prevents Proto.Actor's
/// supervisor restart strategy from firing and ensures callers always receive a reply.
/// </para>
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
            context.Respond(new CallResult(true));
        }
        catch (PrologException ex)
        {
            context.Respond(new CallResult(false, ex.PrologMessage ?? ex.Message));
        }
    }

    private void HandleCall(IContext context, CallMessage msg)
    {
        try
        {
            bool ok = engine.Call(msg.Goal);
            context.Respond(new CallResult(ok));
        }
        catch (PrologException ex)
        {
            context.Respond(new CallResult(false, ex.PrologMessage ?? ex.Message));
        }
    }

    private void HandleQuery(IContext context, QueryMessage msg)
    {
        try
        {
            List<IReadOnlyDictionary<string, string>> solutions = [];

            using PrologQuery query = engine.OpenQuery(msg.Goal);
            foreach (PrologSolution solution in query.Solutions)
            {
                Dictionary<string, string> row = solution.VariableNames
                    .ToDictionary(name => name, name => solution[name].ToString());
                solutions.Add(row);
            }

            context.Respond(new QueryResult(solutions));
        }
        catch (PrologException ex)
        {
            context.Respond(new QueryResult([], ex.PrologMessage ?? ex.Message));
        }
    }

    private void HandleOpenQuery(IContext context, OpenQueryMessage msg)
    {
        try
        {
            PrologQuery query = engine.OpenQuery(msg.Goal);
            Guid id = Guid.NewGuid();
            _openQueries[id] = query;
            context.Respond(new QueryOpenedMessage(id));
        }
        catch (PrologException ex)
        {
            context.Respond(new QueryErrorMessage(ex.PrologMessage ?? ex.Message));
        }
    }

    private void HandleNextSolution(IContext context, NextSolutionMessage msg)
    {
        if (!_openQueries.TryGetValue(msg.QueryId, out PrologQuery? query))
        {
            context.Respond(new QueryEndMessage());
            return;
        }

        try
        {
            if (query.Next())
            {
                Dictionary<string, string> vars = query.Current!.VariableNames
                    .ToDictionary(name => name, name => query.Current[name].ToString());
                context.Respond(new SolutionMessage(vars));
            }
            else
            {
                _openQueries.Remove(msg.QueryId);
                query.Dispose();
                context.Respond(new QueryEndMessage());
            }
        }
        catch (PrologException ex)
        {
            _openQueries.Remove(msg.QueryId);
            query.Dispose();
            context.Respond(new QueryErrorMessage(ex.PrologMessage ?? ex.Message));
        }
    }

    private void HandleCloseQuery(CloseQueryMessage msg)
    {
        if (_openQueries.Remove(msg.QueryId, out PrologQuery? query))
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
}

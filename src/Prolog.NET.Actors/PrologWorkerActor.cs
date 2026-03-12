using Proto;
using Prolog.NET.Swipl;

namespace Prolog.NET.Actors;

/// <summary>
/// The single named <c>"prolog"</c> actor per worker process. Handles one-shot
/// operations (load, call, query) directly via the injected <see cref="PrologEngine"/>,
/// and spawns a new <see cref="PrologQueryActor"/> child for each streaming query.
/// </summary>
/// <remarks>
/// Concurrency is capped at <c>PROLOG_ENGINE_THREADS</c> simultaneous open queries
/// (defaults to 1). When the cap is reached, <see cref="OpenQueryMessage"/> is rejected
/// with <c>Failed { "No capacity" }</c> — the server-level router then decides whether
/// to spawn a new worker process for the same file.
/// </remarks>
public sealed class PrologWorkerActor(PrologEngine engine) : IActor
{
    private readonly int _maxConcurrentQueries = int.TryParse(
        Environment.GetEnvironmentVariable("PROLOG_ENGINE_THREADS"), out int n) && n > 0 ? n : 1;

    private readonly Dictionary<Guid, PID> _queryToActor = [];

    public async Task ReceiveAsync(IContext context)
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
                await HandleNextSolutionAsync(context, msg);
                break;
            case CloseQueryMessage msg:
                HandleCloseQuery(context, msg);
                break;
            case Terminated terminated:
                CleanUpTerminated(terminated.Who);
                break;
        }
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
            QueryResult result = new();
            using PrologQuery query = engine.OpenQuery(msg.Goal);
            foreach (PrologSolution solution in query.Solutions)
            {
                SolutionRow row = new();
                foreach (string name in solution.VariableNames)
                {
                    row.Variables[name] = solution[name].ToString();
                }
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
        if (_queryToActor.Count >= _maxConcurrentQueries)
        {
            context.Respond(new OpenQueryResponse
            {
                Failed = new OpenQueryFailedResult { Error = "No capacity" }
            });
            return;
        }

        try
        {
            PrologQuery query = engine.OpenQuery(msg.Goal);
            Guid id = Guid.NewGuid();
            Props props = Props.FromProducer(() => new PrologQueryActor(query));
            PID queryActorPid = context.Spawn(props);
            context.Watch(queryActorPid);
            _queryToActor[id] = queryActorPid;
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

    private async Task HandleNextSolutionAsync(IContext context, NextSolutionMessage msg)
    {
        if (!Guid.TryParse(msg.QueryId, out Guid id) || !_queryToActor.TryGetValue(id, out PID? queryActorPid))
        {
            context.Respond(new NextSolutionResponse { NoMore = new NoMoreSolutionsResult() });
            return;
        }

        using CancellationTokenSource cts = new(TimeSpan.FromSeconds(30));
        NextSolutionResponse response = await context.System.Root.RequestAsync<NextSolutionResponse>(
            queryActorPid, msg, cts.Token);

        bool done = response.ResultCase is
            NextSolutionResponse.ResultOneofCase.FinalSolution or
            NextSolutionResponse.ResultOneofCase.NoMore or
            NextSolutionResponse.ResultOneofCase.Failed;

        if (done)
        {
            _queryToActor.Remove(id);
        }

        context.Respond(response);
    }

    private void HandleCloseQuery(IContext context, CloseQueryMessage msg)
    {
        if (Guid.TryParse(msg.QueryId, out Guid id) && _queryToActor.Remove(id, out PID? queryActorPid))
        {
            context.System.Root.Send(queryActorPid, msg);
        }
    }

    /// <summary>
    /// Removes the dictionary entry for a query actor that terminated unexpectedly
    /// (i.e. before <see cref="HandleNextSolutionAsync"/> had a chance to clean it up).
    /// </summary>
    private void CleanUpTerminated(PID terminated)
    {
        foreach ((Guid id, PID pid) in _queryToActor)
        {
            if (pid.Equals(terminated))
            {
                _queryToActor.Remove(id);
                return;
            }
        }
    }
}

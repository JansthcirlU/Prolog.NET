using Microsoft.Extensions.DependencyInjection;
using Proto;

namespace Prolog.NET.Actors;

/// <summary>
/// The single named <c>"prolog"</c> actor per worker process. Manages a pool of
/// <see cref="PrologActor"/> children, routing one-shot messages to any idle actor
/// and pinning streaming queries to a dedicated actor for their lifetime.
/// </summary>
public sealed class PrologWorkerActor(IServiceProvider sp) : IActor
{
    private readonly int _poolSize = int.TryParse(
        Environment.GetEnvironmentVariable("PROLOG_ACTOR_POOL_SIZE"), out int n) && n > 0 ? n : 1;
    private readonly Queue<PID> _idleActors = new();
    private readonly Dictionary<Guid, PID> _queryToActor = [];

    public async Task ReceiveAsync(IContext context)
    {
        switch (context.Message)
        {
            case Started:
                for (int i = 0; i < _poolSize; i++)
                {
                    Props props = Props.FromProducer(sp.GetRequiredService<PrologActor>);
                    _idleActors.Enqueue(context.Spawn(props));
                }
                break;
            case LoadFileMessage msg:
                await ForwardToIdleActorAsync<CallResult>(context, msg);
                break;
            case CallMessage msg:
                await ForwardToIdleActorAsync<CallResult>(context, msg);
                break;
            case QueryMessage msg:
                await ForwardToIdleActorAsync<QueryResult>(context, msg);
                break;
            case OpenQueryMessage msg:
                await HandleOpenQueryAsync(context, msg);
                break;
            case NextSolutionMessage msg:
                await HandleNextSolutionAsync(context, msg);
                break;
            case CloseQueryMessage msg:
                HandleCloseQuery(context, msg);
                break;
        }
    }

    private async Task ForwardToIdleActorAsync<TResponse>(IContext context, object message)
        where TResponse : class
    {
        if (!_idleActors.TryDequeue(out PID? actor))
        {
            RespondWithNoActorError(context, message);
            return;
        }

        try
        {
            using CancellationTokenSource cts = new(TimeSpan.FromSeconds(30));
            TResponse response = await context.System.Root.RequestAsync<TResponse>(actor, message, cts.Token);
            context.Respond(response);
        }
        finally { _idleActors.Enqueue(actor); }
    }

    private static void RespondWithNoActorError(IContext context, object message)
    {
        const string error = "No idle actor available";
        object response = message switch
        {
            QueryMessage => new QueryResult { ErrorMessage = error },
            _ => new CallResult { Success = false, ErrorMessage = error },
        };
        context.Respond(response);
    }

    private async Task HandleOpenQueryAsync(IContext context, OpenQueryMessage msg)
    {
        if (!_idleActors.TryDequeue(out PID? actor))
        {
            context.Respond(new OpenQueryResponse
            {
                Failed = new OpenQueryFailedResult { Error = "No idle actor available" }
            });
            return;
        }

        using CancellationTokenSource cts = new(TimeSpan.FromSeconds(30));
        OpenQueryResponse response = await context.System.Root.RequestAsync<OpenQueryResponse>(
            actor, msg, cts.Token);

        if (response.ResultCase == OpenQueryResponse.ResultOneofCase.Opened
            && Guid.TryParse(response.Opened.QueryId, out Guid id))
        {
            _queryToActor[id] = actor;  // actor stays out of idle pool
        }
        else
        {
            _idleActors.Enqueue(actor); // open failed — return actor
        }

        context.Respond(response);
    }

    private async Task HandleNextSolutionAsync(IContext context, NextSolutionMessage msg)
    {
        if (!Guid.TryParse(msg.QueryId, out Guid id) || !_queryToActor.TryGetValue(id, out PID? actor))
        {
            context.Respond(new NextSolutionResponse { NoMore = new NoMoreSolutionsResult() });
            return;
        }

        using CancellationTokenSource cts = new(TimeSpan.FromSeconds(30));
        NextSolutionResponse response = await context.System.Root.RequestAsync<NextSolutionResponse>(
            actor, msg, cts.Token);

        bool done = response.ResultCase is
            NextSolutionResponse.ResultOneofCase.FinalSolution or
            NextSolutionResponse.ResultOneofCase.NoMore or
            NextSolutionResponse.ResultOneofCase.Failed;

        if (done)
        {
            _queryToActor.Remove(id);
            _idleActors.Enqueue(actor);
        }

        context.Respond(response);
    }

    private void HandleCloseQuery(IContext context, CloseQueryMessage msg)
    {
        if (Guid.TryParse(msg.QueryId, out Guid id) && _queryToActor.Remove(id, out PID? actor))
        {
            context.System.Root.Send(actor, msg);
            _idleActors.Enqueue(actor);
        }
    }
}

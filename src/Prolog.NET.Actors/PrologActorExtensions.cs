using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Proto;
using Prolog.NET.Swipl;
using Proto.DependencyInjection;

namespace Prolog.NET.Actors;

/// <summary>
/// Extension methods for registering Proto.Actor services with
/// Microsoft.Extensions.DependencyInjection and for streaming Prolog queries.
/// </summary>
public static class PrologActorExtensions
{
    /// <summary>
    /// Registers the Proto.Actor <see cref="ActorSystem"/> as a singleton (with DI support
    /// wired via <c>WithServiceProvider</c>) and <see cref="PrologActor"/> as a transient
    /// service so Proto.Actor can resolve and inject it per spawn.
    /// </summary>
    public static IServiceCollection AddPrologActors(this IServiceCollection services)
    {
        services.AddSingleton(sp => new ActorSystem().WithServiceProvider(sp));
        services.AddTransient<PrologActor>();
        return services;
    }

    /// <summary>
    /// Streams solutions for <paramref name="goal"/> from the <see cref="PrologActor"/>
    /// at <paramref name="pid"/> one at a time, using the pull-based streaming protocol.
    /// </summary>
    /// <remarks>
    /// The consumer controls the pace. Cancelling the enumeration sends a
    /// <see cref="CloseQueryMessage"/> to cleanly close the query on the actor side.
    /// </remarks>
    public static async IAsyncEnumerable<IReadOnlyDictionary<string, string>> QueryAsync(
        this IRootContext root,
        PID pid,
        string goal,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        object opened = await root.RequestAsync<object>(pid, new OpenQueryMessage(goal), ct);

        if (opened is QueryErrorMessage err)
        {
            throw new PrologException(err.Error);
        }

        Guid queryId = ((QueryOpenedMessage)opened).QueryId;

        try
        {
            while (!ct.IsCancellationRequested)
            {
                object next = await root.RequestAsync<object>(
                    pid, new NextSolutionMessage(queryId), ct);

                if (next is SolutionMessage sol)
                {
                    yield return sol.Variables;
                }
                else if (next is QueryEndMessage)
                {
                    yield break;
                }
                else if (next is QueryErrorMessage qerr)
                {
                    throw new PrologException(qerr.Error);
                }
            }
        }
        finally
        {
            root.Send(pid, new CloseQueryMessage(queryId));
        }
    }
}

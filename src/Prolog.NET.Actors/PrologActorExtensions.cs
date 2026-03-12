using Microsoft.Extensions.DependencyInjection;
using Proto;
using Proto.DependencyInjection;
using Proto.Remote;
using Proto.Remote.GrpcNet;
using static Proto.Remote.RemoteConfig;

namespace Prolog.NET.Actors;

/// <summary>
/// Extension methods for registering Proto.Actor services with
/// Microsoft.Extensions.DependencyInjection.
/// </summary>
public static class PrologActorExtensions
{
    /// <summary>
    /// Registers the Proto.Actor <see cref="ActorSystem"/> with Proto.Remote support (for
    /// use in worker processes) and <see cref="PrologWorkerActor"/> as a transient service.
    /// </summary>
    /// <param name="port">The port the remote listener will bind to.</param>
    public static IServiceCollection AddPrologActors(this IServiceCollection services, int port)
        => services
            .AddSingleton(sp => new ActorSystem()
                .WithServiceProvider(sp)
                .WithRemote(BindToLocalhost(port)
                    .WithProtoMessages(MessagesReflection.Descriptor)))
            .AddTransient<PrologWorkerActor>();

    /// <summary>
    /// Registers the Proto.Actor <see cref="ActorSystem"/> with Proto.Remote support (for
    /// use in the server process) bound to <paramref name="port"/>. No named actors are
    /// spawned — the server acts as a client that routes messages to remote workers.
    /// </summary>
    /// <param name="port">The port the server's remote listener will bind to.</param>
    public static IServiceCollection AddPrologServerActors(this IServiceCollection services, int port = 4000)
        => services
            .AddSingleton(sp => new ActorSystem()
                .WithServiceProvider(sp)
                .WithRemote(BindToLocalhost(port)
                    .WithProtoMessages(MessagesReflection.Descriptor)));
}

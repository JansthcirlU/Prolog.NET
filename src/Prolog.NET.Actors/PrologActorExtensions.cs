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
    /// use in worker processes) and <see cref="PrologActor"/> as a transient service.
    /// </summary>
    /// <param name="port">The port the remote listener will bind to.</param>
    public static IServiceCollection AddPrologActors(this IServiceCollection services, int port)
        => services
            .AddSingleton(sp => new ActorSystem()
                .WithServiceProvider(sp)
                .WithRemote(BindToLocalhost(port)
                    .WithProtoMessages(MessagesReflection.Descriptor)))
            .AddTransient<PrologActor>();

    /// <summary>
    /// Registers the console-side <see cref="ActorSystem"/> with Proto.Remote (listening on
    /// <paramref name="port"/>) and <see cref="CliActor"/> as a transient service.
    /// </summary>
    /// <param name="port">The port the console process will listen on for remote replies.</param>
    public static IServiceCollection AddCliActor(this IServiceCollection services, int port = 4000)
        => services
            .AddSingleton(sp => new ActorSystem()
                .WithServiceProvider(sp)
                .WithRemote(BindToLocalhost(port)
                    .WithProtoMessages(MessagesReflection.Descriptor)))
            .AddTransient<CliActor>();
}

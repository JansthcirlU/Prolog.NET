using Microsoft.Extensions.DependencyInjection;
using Proto;
using Proto.DependencyInjection;

namespace Prolog.NET.Actors;

/// <summary>
/// Extension methods for registering Proto.Actor services with
/// Microsoft.Extensions.DependencyInjection.
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
}

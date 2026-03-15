using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Proto;
using Proto.Remote;
using Prolog.NET.Actors;

namespace Prolog.NET.Worker;

/// <summary>
/// Hosted service that starts the Proto.Remote listener and spawns the named
/// <see cref="PrologWorkerActor"/> so remote callers can reach it by name.
/// </summary>
internal sealed class PrologWorker(ActorSystem actorSystem, IServiceProvider sp) : IHostedService
{
    private PID? _workerActorPid;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await actorSystem.Remote().StartAsync();
        Props props = Props.FromProducer(sp.GetRequiredService<PrologWorkerActor>);
        _workerActorPid = actorSystem.Root.SpawnNamed(props, "prolog");
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_workerActorPid != null)
        {
            await actorSystem.Root.StopAsync(_workerActorPid);
        }

        await actorSystem.Remote().ShutdownAsync();
    }
}

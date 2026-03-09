using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Proto;
using Proto.Remote;
using Prolog.NET.Actors;

namespace Prolog.NET.Worker;

/// <summary>
/// Hosted service that starts the Proto.Remote listener and spawns the named
/// <see cref="PrologActor"/> so remote callers can reach it by name.
/// </summary>
internal sealed class WorkerHost(ActorSystem actorSystem, IServiceProvider sp) : IHostedService
{
    private PID? _prologPid;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await actorSystem.Remote().StartAsync();
        Props props = Props.FromProducer(sp.GetRequiredService<PrologActor>);
        _prologPid = actorSystem.Root.SpawnNamed(props, "prolog");
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_prologPid != null)
        {
            await actorSystem.Root.StopAsync(_prologPid);
        }

        await actorSystem.Remote().ShutdownAsync();
    }
}

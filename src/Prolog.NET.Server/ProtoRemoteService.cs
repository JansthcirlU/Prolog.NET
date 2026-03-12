using Microsoft.Extensions.Hosting;
using Proto;
using Proto.Remote;

namespace Prolog.NET.Server;

/// <summary>
/// Hosted service that starts and stops the Proto.Remote listener used for
/// server-to-worker communication.
/// </summary>
internal sealed class ProtoRemoteService(ActorSystem actorSystem) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
        => actorSystem.Remote().StartAsync();

    public Task StopAsync(CancellationToken cancellationToken)
        => actorSystem.Remote().ShutdownAsync();
}

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Proto;
using Prolog.NET.Actors;

namespace Prolog.NET.Console;

/// <summary>
/// A hosted background service that spawns a <see cref="PrologActor"/> and demonstrates
/// basic request-reply interaction with the Prolog engine.
/// </summary>
public sealed class PrologWorker(
    ActorSystem actorSystem,
    IServiceProvider serviceProvider,
    ILogger<PrologWorker> logger) : BackgroundService
{
    private PID? _pid;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Props props = Props.FromProducer(serviceProvider.GetRequiredService<PrologActor>);
        _pid = actorSystem.Root.Spawn(props);
        logger.LogInformation("PrologActor started with PID {Pid}", _pid);

        // Assert a fact
        CallResult assertResult = await actorSystem.Root.RequestAsync<CallResult>(
            _pid, new CallMessage("assert(likes(bob, alice))"), stoppingToken);

        if (!assertResult.Success)
        {
            logger.LogError("assert failed: {Error}", assertResult.ErrorMessage);
        }

        // Query it back (lazy streaming)
        await foreach (IReadOnlyDictionary<string, string> solution in actorSystem.Root.QueryAsync(_pid, "likes(bob, X)", stoppingToken))
        {
            logger.LogInformation("Solution: X = {X}", solution["X"]);
        }

        // Keep the service running until Ctrl+C
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_pid is not null)
        {
            await actorSystem.Root.StopAsync(_pid);
        }

        await base.StopAsync(cancellationToken);
    }
}

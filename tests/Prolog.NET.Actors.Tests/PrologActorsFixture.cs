using Microsoft.Extensions.DependencyInjection;
using Proto;
using Proto.DependencyInjection;
using Prolog.NET.Swipl;

namespace Prolog.NET.Actors.Tests;

[CollectionDefinition("PrologActors")]
public class PrologActorsCollectionDefinition : ICollectionFixture<PrologActorsFixture> { }

public sealed class PrologActorsFixture : IAsyncLifetime
{
    private ServiceProvider _services = null!;

    public ActorSystem ActorSystem { get; private set; } = null!;
    public PID SingleWorkerPid { get; private set; } = null!;
    public string PrologFilePath { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        ServiceCollection sc = new();
        sc.AddPrologEngine();
        sc.AddTransient<PrologWorkerActor>();
        sc.AddSingleton(sp => new ActorSystem().WithServiceProvider(sp));

        _services = sc.BuildServiceProvider();
        ActorSystem = _services.GetRequiredService<ActorSystem>();

        Environment.SetEnvironmentVariable("PROLOG_ENGINE_THREADS", "1");
        SingleWorkerPid = ActorSystem.Root.SpawnNamed(
            Props.FromProducer(_services.GetRequiredService<PrologWorkerActor>),
            "prolog-single");
        using (CancellationTokenSource pingCts = new(TimeSpan.FromSeconds(15)))
        {
            await ActorSystem.Root.RequestAsync<CallResult>(
                SingleWorkerPid, new CallMessage { Goal = "true" }, pingCts.Token);
        }

        PrologFilePath = Path.Combine(Path.GetTempPath(), "family.pl");
        await File.WriteAllTextAsync(PrologFilePath, """
            parent(tom, bob).  parent(tom, liz).
            parent(bob, ann).  parent(bob, pat).
            ancestor(X, Y) :- parent(X, Y).
            ancestor(X, Y) :- parent(X, Z), ancestor(Z, Y).
            """);
    }

    public async Task DisposeAsync()
    {
        await ActorSystem.Root.StopAsync(SingleWorkerPid);
        await _services.DisposeAsync();

        if (PrologFilePath is not null && File.Exists(PrologFilePath))
        {
            File.Delete(PrologFilePath);
        }
    }
}

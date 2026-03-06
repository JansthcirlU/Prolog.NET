using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Proto;
using Prolog.NET.Actors;

namespace Prolog.NET.Console;

/// <summary>
/// A hosted background service that spawns a <see cref="PrologActor"/> and demonstrates
/// the raw lazy-streaming protocol with contextual command hints at each step.
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
        logger.LogInformation(
            "PrologActor started — try: CallMessage | LoadFileMessage | OpenQueryMessage");

        // Assert a fact
        logger.LogInformation("Asserting likes(bob, alice)...");
        CallResult assertResult = await actorSystem.Root.RequestAsync<CallResult>(
            _pid, new CallMessage("assert(likes(bob, alice))"), stoppingToken);

        if (!assertResult.Success)
        {
            logger.LogError("assert failed: {Error}", assertResult.ErrorMessage);
            return;
        }

        logger.LogInformation("assert succeeded — try: CallMessage | OpenQueryMessage");

        // Open a streaming query
        const string goal = "likes(bob, X)";
        logger.LogInformation("Opening query: {Goal}", goal);

        OpenQueryResult opened = await actorSystem.Root.RequestAsync<OpenQueryResult>(
            _pid, new OpenQueryMessage(goal), stoppingToken);

        if (opened is OpenQueryFailedResult openFailed)
        {
            logger.LogError("OpenQuery failed: {Error}", openFailed.Error);
            return;
        }

        Guid queryId = ((QueryOpenedResult)opened).QueryId;
        logger.LogInformation(
            "Query opened (id: {QueryId}) — send NextSolutionMessage to fetch first solution",
            queryId);

        // Pull solutions one at a time
        while (true)
        {
            NextSolutionResult next = await actorSystem.Root.RequestAsync<NextSolutionResult>(
                _pid, new NextSolutionMessage(queryId), stoppingToken);

            switch (next)
            {
                case SolutionResult sol:
                    string solVars = FormatVariables(sol.Variables);
                    logger.LogInformation("Solution: {Vars}  [more may follow]", solVars);
                    logger.LogInformation(
                        "  → open query {QueryId} — send NextSolutionMessage or CloseQueryMessage",
                        queryId);
                    break;

                case FinalSolutionResult final:
                    string finalVars = FormatVariables(final.Variables);
                    logger.LogInformation(
                        "Solution: {Vars}  [FinalSolution — query closed automatically]",
                        finalVars);
                    logger.LogInformation(
                        "  → no open query — try: OpenQueryMessage | CallMessage");
                    goto done;

                case NoMoreSolutionsResult:
                    logger.LogInformation("No more solutions — query closed automatically");
                    logger.LogInformation(
                        "  → no open query — try: OpenQueryMessage | CallMessage");
                    goto done;

                case QueryFailedResult failed:
                    logger.LogError(
                        "Query error: {Error} — query closed automatically", failed.Error);
                    logger.LogInformation(
                        "  → no open query — try: OpenQueryMessage | CallMessage");
                    goto done;
            }
        }

        done:
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

    private static string FormatVariables(IReadOnlyDictionary<string, string> variables)
    {
        return string.Join(", ", variables.Select(kv => $"{kv.Key} = {kv.Value}"));
    }
}

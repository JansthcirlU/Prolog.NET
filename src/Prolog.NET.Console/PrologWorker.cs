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
public sealed partial class PrologWorker(
    ActorSystem actorSystem,
    IServiceProvider serviceProvider,
    ILogger<PrologWorker> logger,
    IHostApplicationLifetime lifetime) : BackgroundService
{
    private PID? _pid;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Props props = Props.FromProducer(serviceProvider.GetRequiredService<PrologActor>);
        _pid = actorSystem.Root.Spawn(props);
        LogActorStarted();

        // Assert a fact
        LogAssertingFact();
        CallResult assertResult = await actorSystem.Root.RequestAsync<CallResult>(
            _pid, new CallMessage("assert(likes(bob, alice))"), stoppingToken);

        if (!assertResult.Success)
        {
            LogAssertFailed(assertResult.ErrorMessage);
            return;
        }

        LogAssertSucceeded();

        // Open a streaming query
        const string goal = "likes(bob, X)";
        LogOpeningQuery(goal);

        OpenQueryResult opened = await actorSystem.Root.RequestAsync<OpenQueryResult>(
            _pid, new OpenQueryMessage(goal), stoppingToken);

        if (opened is OpenQueryFailedResult openFailed)
        {
            LogOpenQueryFailed(openFailed.Error);
            return;
        }

        Guid queryId = ((QueryOpenedResult)opened).QueryId;
        LogQueryOpened(queryId);

        // Pull solutions one at a time
        while (true)
        {
            NextSolutionResult next = await actorSystem.Root.RequestAsync<NextSolutionResult>(
                _pid, new NextSolutionMessage(queryId), stoppingToken);

            switch (next)
            {
                case SolutionResult sol:
                    LogSolutionMore(FormatVariables(sol.Variables));
                    LogQueryOpenHint(queryId);
                    break;

                case FinalSolutionResult final:
                    LogFinalSolution(FormatVariables(final.Variables));
                    LogNoOpenQueryHint();
                    goto done;

                case NoMoreSolutionsResult:
                    LogNoMoreSolutions();
                    LogNoOpenQueryHint();
                    goto done;

                case QueryFailedResult failed:
                    LogQueryError(failed.Error);
                    LogNoOpenQueryHint();
                    goto done;
            }
        }

        done:
        // Demo complete — signal the host to shut down gracefully.
        lifetime.StopApplication();
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
        => string.Join(", ", variables.Select(kv => $"{kv.Key} = {kv.Value}"));

    [LoggerMessage(Level = LogLevel.Information, Message = "PrologActor started — try: CallMessage | LoadFileMessage | OpenQueryMessage")]
    private partial void LogActorStarted();

    [LoggerMessage(Level = LogLevel.Information, Message = "Asserting likes(bob, alice)...")]
    private partial void LogAssertingFact();

    [LoggerMessage(Level = LogLevel.Error, Message = "assert failed: {Error}")]
    private partial void LogAssertFailed(string? error);

    [LoggerMessage(Level = LogLevel.Information, Message = "assert succeeded — try: CallMessage | OpenQueryMessage")]
    private partial void LogAssertSucceeded();

    [LoggerMessage(Level = LogLevel.Information, Message = "Opening query: {Goal}")]
    private partial void LogOpeningQuery(string goal);

    [LoggerMessage(Level = LogLevel.Error, Message = "OpenQuery failed: {Error}")]
    private partial void LogOpenQueryFailed(string error);

    [LoggerMessage(Level = LogLevel.Information, Message = "Query opened (id: {QueryId}) — send NextSolutionMessage to fetch first solution")]
    private partial void LogQueryOpened(Guid queryId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Solution: {Vars}  [more may follow]")]
    private partial void LogSolutionMore(string vars);

    [LoggerMessage(Level = LogLevel.Information, Message = "  → open query {QueryId} — send NextSolutionMessage or CloseQueryMessage")]
    private partial void LogQueryOpenHint(Guid queryId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Solution: {Vars}  [FinalSolution — query closed automatically]")]
    private partial void LogFinalSolution(string vars);

    [LoggerMessage(Level = LogLevel.Information, Message = "  → no open query — try: OpenQueryMessage | CallMessage")]
    private partial void LogNoOpenQueryHint();

    [LoggerMessage(Level = LogLevel.Information, Message = "No more solutions — query closed automatically")]
    private partial void LogNoMoreSolutions();

    [LoggerMessage(Level = LogLevel.Error, Message = "Query error: {Error} — query closed automatically")]
    private partial void LogQueryError(string error);
}

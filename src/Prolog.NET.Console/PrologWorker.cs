using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Proto;
using Proto.Remote;
using Prolog.NET.Actors;

namespace Prolog.NET.Console;

/// <summary>
/// Interactive background service. Starts the remote listener, spawns a <see cref="CliActor"/>,
/// then drives a keystroke-based console UI. All state and allowed actions come from
/// <see cref="CliResponse"/> — this class never hardcodes what the user can do per state.
/// </summary>
public sealed class PrologWorker(
    ActorSystem actorSystem,
    IServiceProvider serviceProvider,
    IHostApplicationLifetime lifetime) : BackgroundService
{
    private PID? _cliPid;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await actorSystem.Remote().StartAsync();

        Props props = Props.FromProducer(serviceProvider.GetRequiredService<CliActor>);
        _cliPid = actorSystem.Root.Spawn(props);

        // Get the initial state and allowed actions from the actor.
        CliResponse current = await RequestAsync<CliResponse>(new GetStateRequest(), stoppingToken);

        System.Console.Clear();

        string? statusLine = null;

        while (!stoppingToken.IsCancellationRequested)
        {
            RenderHeader(current.Slots, current.ActiveSlot);

            if (statusLine != null)
            {
                System.Console.WriteLine(statusLine);
                statusLine = null;
            }

            System.Console.WriteLine();

            CliResponse? response = null;
            bool halt = false;

            try
            {
                (response, halt) = await HandleInputAsync(current.AllowedActions, stoppingToken);
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex) { statusLine = $"[!] {ex.Message}"; }

            if (halt)
            {
                await RequestAsync<CliResponse>(new HaltRequest(), stoppingToken);
                break;
            }

            if (response != null)
            {
                current    = response;
                statusLine = FormatResponse(response);
                System.Console.Clear();
            }
        }

        lifetime.StopApplication();
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_cliPid != null)
            await actorSystem.Root.StopAsync(_cliPid);

        await actorSystem.Remote().ShutdownAsync();
        await base.StopAsync(cancellationToken);
    }

    // --- Generic input handler ---

    private async Task<(CliResponse? response, bool halt)>
        HandleInputAsync(IReadOnlyList<AllowedAction> allowed, CancellationToken ct)
    {
        // Render hint bar from keyed actions only.
        string hints = string.Join("  ",
            allowed.Where(a => a.KeyHint.HasValue)
                   .Select(a => $"[{a.KeyHint}] {a.Label}"));
        System.Console.WriteLine(hints);
        System.Console.Write("> ");

        ConsoleKeyInfo k = System.Console.ReadKey(intercept: true);
        System.Console.WriteLine();

        // Find matching action by key hint.
        AllowedAction? matched = allowed.FirstOrDefault(a =>
            a.KeyHint.HasValue &&
            char.ToUpperInvariant(k.KeyChar) == char.ToUpperInvariant(a.KeyHint.Value));

        // Fall back to free-text (QueryText) action if no keyed action matched.
        if (matched == null)
        {
            AllowedAction? textAction = allowed.FirstOrDefault(a => a.RequiredInput == ActionInput.QueryText);
            if (textAction != null && k.KeyChar != '\0' && !char.IsControl(k.KeyChar))
                matched = textAction;
        }

        if (matched == null) return (null, false);

        return matched.Action switch
        {
            CliAction.Halt          => (null, true),
            CliAction.LoadFile      => (await LoadFileAsync(matched, ct), false),
            CliAction.UnloadFile    => (await SlotInputAsync(matched, s => new UnloadFileRequest(s), ct), false),
            CliAction.SwitchSlot    => (await SlotInputAsync(matched, s => new SwitchSlotRequest(s), ct), false),
            CliAction.SubmitQuery   => (await QueryAsync(k.KeyChar, ct), false),
            CliAction.NextSolution  => (await RequestAsync<CliResponse>(new NextSolutionRequest(), ct), false),
            CliAction.CloseQuery    => (await RequestAsync<CliResponse>(new CloseQueryRequest(), ct), false),
            _                       => (null, false),
        };
    }

    private async Task<CliResponse?> LoadFileAsync(AllowedAction action, CancellationToken ct)
    {
        System.Console.Write(action.InputPrompt ?? "File path: ");
        string path = (System.Console.ReadLine() ?? "").Trim();
        if (string.IsNullOrEmpty(path)) return null;
        return await RequestAsync<CliResponse>(new LoadFileRequest(path), ct);
    }

    private async Task<CliResponse?> SlotInputAsync(
        AllowedAction action, Func<int, object> makeRequest, CancellationToken ct)
    {
        System.Console.Write(action.InputPrompt ?? "Slot (0-3): ");
        if (int.TryParse(System.Console.ReadLine(), out int slot))
            return await RequestAsync<CliResponse>(makeRequest(slot), ct);
        return null;
    }

    private async Task<CliResponse?> QueryAsync(char firstChar, CancellationToken ct)
    {
        System.Console.Write(firstChar);
        string rest = System.Console.ReadLine() ?? "";
        string goal = $"{firstChar}{rest}";
        return await RequestAsync<CliResponse>(new QueryRequest(goal), ct);
    }

    // --- Helpers ---

    private Task<T> RequestAsync<T>(object message, CancellationToken ct)
        => actorSystem.Root.RequestAsync<T>(_cliPid!, message, ct);

    private static void RenderHeader(SlotInfo[] slots, int? activeSlot)
    {
        string slotBar = string.Join(" | ", slots.Select(s =>
            s.FilePath != null ? Path.GetFileName(s.FilePath) : "(empty)"));

        System.Console.WriteLine($"=== Prolog.NET ===  [{slotBar}]");

        string activeDisplay = activeSlot.HasValue && slots[activeSlot.Value].FilePath != null
            ? Path.GetFileName(slots[activeSlot.Value].FilePath)!
            : "(none)";

        System.Console.WriteLine($"Active: {activeDisplay}");
    }

    private static string? FormatResponse(CliResponse response) => response switch
    {
        CliError err                        => $"[!] {err.Error}",
        CliSolution { IsFinal: true } sol   => $"  {FormatVars(sol.Variables)}  (last solution)",
        CliSolution sol                     => $"  {FormatVars(sol.Variables)}",
        CliNoMoreSolutions                  => "(no more solutions)",
        _                                   => null,
    };

    private static string FormatVars(IReadOnlyDictionary<string, string> variables)
        => variables.Count == 0
            ? "true"
            : string.Join(", ", variables.Select(kv => $"{kv.Key} = {kv.Value}"));
}

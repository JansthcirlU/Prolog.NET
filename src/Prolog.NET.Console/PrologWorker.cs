using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Proto;
using Proto.Remote;
using Prolog.NET.Actors;

namespace Prolog.NET.Console;

/// <summary>
/// Interactive background service. Starts the remote listener, spawns a <see cref="CliActor"/>,
/// then drives a keystroke-based console UI. All state lives in the <see cref="CliActor"/> —
/// every response carries the current <see cref="CliState"/>, slot list, and active slot so
/// this class never has to track them itself.
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

        CliState state = CliState.NoSlot;
        SlotInfo[] slots = EmptySlots();
        int? activeSlot = null;
        string? statusLine = null;

        System.Console.Clear();

        while (!stoppingToken.IsCancellationRequested)
        {
            RenderHeader(slots, activeSlot);

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
                (response, halt) = state switch
                {
                    CliState.Streaming  => await HandleStreamingInputAsync(stoppingToken),
                    CliState.SlotReady  => await HandleSlotReadyInputAsync(stoppingToken),
                    _                   => await HandleNoSlotInputAsync(stoppingToken),
                };
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
                state      = response.State;
                slots      = response.Slots;
                activeSlot = response.ActiveSlot;
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

    // --- Input handlers ---

    private async Task<(CliResponse? response, bool halt)> HandleNoSlotInputAsync(CancellationToken ct)
    {
        System.Console.WriteLine("[L] Load file  [H] Halt");
        ConsoleKeyInfo k = System.Console.ReadKey(intercept: true);
        System.Console.WriteLine();

        return k.Key switch
        {
            ConsoleKey.L => (await HandleLoadAsync(ct), false),
            ConsoleKey.H => (null, true),
            _            => (null, false),
        };
    }

    private async Task<(CliResponse? response, bool halt)> HandleSlotReadyInputAsync(CancellationToken ct)
    {
        System.Console.WriteLine("[L] Load  [U] Unload  [S] Switch  [H] Halt  or type a query:");
        System.Console.Write("> ");
        ConsoleKeyInfo k = System.Console.ReadKey(intercept: true);

        switch (k.Key)
        {
            case ConsoleKey.L: System.Console.WriteLine(); return (await HandleLoadAsync(ct),   false);
            case ConsoleKey.U: System.Console.WriteLine(); return (await HandleUnloadAsync(ct), false);
            case ConsoleKey.S: System.Console.WriteLine(); return (await HandleSwitchAsync(ct), false);
            case ConsoleKey.H: System.Console.WriteLine(); return (null, true);
            case ConsoleKey.Enter:
            case ConsoleKey.Escape:
                System.Console.WriteLine();
                return (null, false);
            default:
                if (k.KeyChar != '\0' && !char.IsControl(k.KeyChar))
                {
                    System.Console.Write(k.KeyChar);
                    string rest = System.Console.ReadLine() ?? "";
                    string goal = $"{k.KeyChar}{rest}";
                    return (await RequestAsync<CliResponse>(new QueryRequest(goal), ct), false);
                }
                return (null, false);
        }
    }

    private async Task<(CliResponse? response, bool halt)> HandleStreamingInputAsync(CancellationToken ct)
    {
        System.Console.WriteLine("[N] Next  [C] Close  [H] Halt");
        ConsoleKeyInfo k = System.Console.ReadKey(intercept: true);
        System.Console.WriteLine();

        return k.Key switch
        {
            ConsoleKey.N => (await RequestAsync<CliResponse>(new NextSolutionRequest(), ct), false),
            ConsoleKey.C => (await RequestAsync<CliResponse>(new CloseQueryRequest(),    ct), false),
            ConsoleKey.H => (null, true),
            _            => (null, false),
        };
    }

    // --- Sub-prompts ---

    private async Task<CliResponse?> HandleLoadAsync(CancellationToken ct)
    {
        System.Console.Write("File path: ");
        string path = (System.Console.ReadLine() ?? "").Trim();
        if (string.IsNullOrEmpty(path)) return null;
        return await RequestAsync<CliResponse>(new LoadFileRequest(path), ct);
    }

    private async Task<CliResponse?> HandleUnloadAsync(CancellationToken ct)
    {
        System.Console.Write("Slot to unload (0-3): ");
        if (int.TryParse(System.Console.ReadLine(), out int slot))
            return await RequestAsync<CliResponse>(new UnloadFileRequest(slot), ct);
        return null;
    }

    private async Task<CliResponse?> HandleSwitchAsync(CancellationToken ct)
    {
        System.Console.Write("Switch to slot (0-3): ");
        if (int.TryParse(System.Console.ReadLine(), out int slot))
            return await RequestAsync<CliResponse>(new SwitchSlotRequest(slot), ct);
        return null;
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

    private static SlotInfo[] EmptySlots() =>
        Enumerable.Range(0, 4).Select(i => new SlotInfo(i, null)).ToArray();
}

using Proto;
using SystemProcess = System.Diagnostics.Process;
using SystemProcessStartInfo = System.Diagnostics.ProcessStartInfo;

namespace Prolog.NET.Actors;

/// <summary>
/// Orchestrates up to 4 <see cref="PrologActor"/> instances running in child worker
/// processes. Each slot corresponds to one loaded knowledge base. Remote communication
/// uses Proto.Remote with Protocol Buffers.
/// </summary>
public sealed class CliActor : IActor
{
    private const int MaxSlots = 4;
    private const int BaseWorkerPort = 4001;
    private const string PrologActorName = "prolog";

    private readonly record struct SlotState(PID RemotePid, string FilePath, SystemProcess Process);

    private readonly Dictionary<int, SlotState> _slots = [];
    private int? _activeSlot;
    private string? _openQueryId;

    private static readonly IReadOnlyList<AllowedAction> NoSlotActions =
    [
        new(CliAction.LoadFile, "Load file", 'L', ActionInput.FilePath, "File path: "),
        new(CliAction.Halt,     "Halt",      'H', ActionInput.None),
    ];

    private static readonly IReadOnlyList<AllowedAction> SlotReadyActions =
    [
        new(CliAction.LoadFile,    "Load",         'L', ActionInput.FilePath,  "File path: "),
        new(CliAction.UnloadFile,  "Unload",       'U', ActionInput.SlotNumber, "Slot (0-3): "),
        new(CliAction.SwitchSlot,  "Switch",       'S', ActionInput.SlotNumber, "Slot (0-3): "),
        new(CliAction.Halt,        "Halt",         'H', ActionInput.None),
        new(CliAction.SubmitQuery, "type a query", null, ActionInput.QueryText),
    ];

    private static readonly IReadOnlyList<AllowedAction> StreamingActions =
    [
        new(CliAction.NextSolution, "Next",  'N', ActionInput.None),
        new(CliAction.CloseQuery,   "Close", 'C', ActionInput.None),
        new(CliAction.Halt,         "Halt",  'H', ActionInput.None),
    ];

    private IReadOnlyList<AllowedAction> GetAllowedActions() => GetState() switch
    {
        CliState.NoSlot    => NoSlotActions,
        CliState.SlotReady => SlotReadyActions,
        CliState.Streaming => StreamingActions,
        _                  => [],
    };

    public async Task ReceiveAsync(IContext context)
    {
        switch (context.Message)
        {
            case GetStateRequest:
                context.Respond(new CliOk(GetState(), GetSlotInfos(), _activeSlot, GetAllowedActions()));
                break;
            case LoadFileRequest msg:
                await HandleLoadFileAsync(context, msg);
                break;
            case UnloadFileRequest msg:
                HandleUnloadFile(context, msg);
                break;
            case SwitchSlotRequest msg:
                HandleSwitchSlot(context, msg);
                break;
            case QueryRequest msg:
                await HandleQueryAsync(context, msg);
                break;
            case NextSolutionRequest:
                await HandleNextSolutionAsync(context);
                break;
            case CloseQueryRequest:
                HandleCloseQuery(context);
                break;
            case HaltRequest:
                HandleHalt(context);
                break;
            case Stopping:
                KillAllWorkers();
                break;
        }
    }

    private async Task HandleLoadFileAsync(IContext context, LoadFileRequest msg)
    {
        if (_slots.Count >= MaxSlots)
        {
            context.Respond(MakeError("All 4 slots are occupied"));
            return;
        }

        int slot = Enumerable.Range(0, MaxSlots).First(i => !_slots.ContainsKey(i));
        int port = BaseWorkerPort + slot;
        SystemProcess? process = null;

        try
        {
            process = StartWorkerProcess(port);
            // Give the worker's gRPC listener time to start.
            await Task.Delay(1500);

            PID remotePid = PID.FromAddress($"127.0.0.1:{port}", PrologActorName);

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            CallResult result = await context.System.Root.RequestAsync<CallResult>(
                remotePid,
                new LoadFileMessage { Path = msg.Path },
                cts.Token);

            if (!result.Success)
            {
                process?.Kill();
                process?.Dispose();
                context.Respond(MakeError(result.ErrorMessage));
                return;
            }

            _slots[slot] = new SlotState(remotePid, msg.Path, process!);
            _activeSlot = slot;
            context.Respond(new CliOk(GetState(), GetSlotInfos(), _activeSlot, GetAllowedActions()));
        }
        catch (Exception ex)
        {
            try { process?.Kill(); process?.Dispose(); } catch { }
            context.Respond(MakeError(ex.Message));
        }
    }

    private void HandleUnloadFile(IContext context, UnloadFileRequest msg)
    {
        if (!_slots.TryGetValue(msg.Slot, out SlotState slot))
        {
            context.Respond(MakeError($"Slot {msg.Slot} is empty"));
            return;
        }

        if (_activeSlot == msg.Slot && _openQueryId != null)
        {
            context.System.Root.Send(slot.RemotePid, new CloseQueryMessage { QueryId = _openQueryId });
            _openQueryId = null;
        }

        try { slot.Process.Kill(); } catch { }
        slot.Process.Dispose();
        _slots.Remove(msg.Slot);

        if (_activeSlot == msg.Slot)
            _activeSlot = _slots.Count > 0 ? _slots.Keys.Min() : null;

        context.Respond(new CliOk(GetState(), GetSlotInfos(), _activeSlot, GetAllowedActions()));
    }

    private void HandleSwitchSlot(IContext context, SwitchSlotRequest msg)
    {
        if (!_slots.ContainsKey(msg.Slot))
        {
            context.Respond(MakeError($"Slot {msg.Slot} is empty"));
            return;
        }

        // Close any open query on the current slot before switching.
        if (_activeSlot.HasValue && _openQueryId != null
            && _slots.TryGetValue(_activeSlot.Value, out SlotState current))
        {
            context.System.Root.Send(current.RemotePid, new CloseQueryMessage { QueryId = _openQueryId });
            _openQueryId = null;
        }

        _activeSlot = msg.Slot;
        context.Respond(new CliOk(GetState(), GetSlotInfos(), _activeSlot, GetAllowedActions()));
    }

    private async Task HandleQueryAsync(IContext context, QueryRequest msg)
    {
        if (!TryGetActiveSlot(out SlotState slot))
        {
            context.Respond(MakeError("No active slot"));
            return;
        }

        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

            OpenQueryResponse opened = await context.System.Root.RequestAsync<OpenQueryResponse>(
                slot.RemotePid,
                new OpenQueryMessage { Goal = msg.Goal },
                cts.Token);

            if (opened.ResultCase == OpenQueryResponse.ResultOneofCase.Failed)
            {
                context.Respond(MakeError(opened.Failed.Error));
                return;
            }

            _openQueryId = opened.Opened.QueryId;

            // Fetch the first solution immediately.
            NextSolutionResponse next = await context.System.Root.RequestAsync<NextSolutionResponse>(
                slot.RemotePid,
                new NextSolutionMessage { QueryId = _openQueryId },
                cts.Token);

            context.Respond(TranslateNextSolution(next));
        }
        catch (Exception ex)
        {
            _openQueryId = null;
            context.Respond(MakeError(ex.Message));
        }
    }

    private async Task HandleNextSolutionAsync(IContext context)
    {
        if (!TryGetActiveSlot(out SlotState slot) || _openQueryId == null)
        {
            context.Respond(MakeError("No active query"));
            return;
        }

        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            NextSolutionResponse next = await context.System.Root.RequestAsync<NextSolutionResponse>(
                slot.RemotePid,
                new NextSolutionMessage { QueryId = _openQueryId },
                cts.Token);

            context.Respond(TranslateNextSolution(next));
        }
        catch (Exception ex)
        {
            _openQueryId = null;
            context.Respond(MakeError(ex.Message));
        }
    }

    private void HandleCloseQuery(IContext context)
    {
        if (TryGetActiveSlot(out SlotState slot) && _openQueryId != null)
        {
            context.System.Root.Send(slot.RemotePid, new CloseQueryMessage { QueryId = _openQueryId });
            _openQueryId = null;
        }

        context.Respond(new CliOk(GetState(), GetSlotInfos(), _activeSlot, GetAllowedActions()));
    }

    private void HandleHalt(IContext context)
    {
        KillAllWorkers();
        context.Respond(new CliOk(CliState.NoSlot, GetSlotInfos(), null, NoSlotActions));
    }

    private void KillAllWorkers()
    {
        foreach (SlotState slot in _slots.Values)
        {
            try { slot.Process.Kill(); } catch { }
            slot.Process.Dispose();
        }

        _slots.Clear();
        _activeSlot = null;
        _openQueryId = null;
    }

    private CliResponse TranslateNextSolution(NextSolutionResponse response)
    {
        switch (response.ResultCase)
        {
            case NextSolutionResponse.ResultOneofCase.Solution:
                return new CliSolution(
                    new Dictionary<string, string>(response.Solution.Variables),
                    false, GetState(), GetSlotInfos(), _activeSlot, GetAllowedActions());

            case NextSolutionResponse.ResultOneofCase.FinalSolution:
                _openQueryId = null;
                return new CliSolution(
                    new Dictionary<string, string>(response.FinalSolution.Variables),
                    true, GetState(), GetSlotInfos(), _activeSlot, GetAllowedActions());

            case NextSolutionResponse.ResultOneofCase.NoMore:
                _openQueryId = null;
                return new CliNoMoreSolutions(GetState(), GetSlotInfos(), _activeSlot, GetAllowedActions());

            case NextSolutionResponse.ResultOneofCase.Failed:
                _openQueryId = null;
                return new CliError(response.Failed.Error, GetState(), GetSlotInfos(), _activeSlot, GetAllowedActions());

            default:
                _openQueryId = null;
                return new CliError("Unexpected response from worker", GetState(), GetSlotInfos(), _activeSlot, GetAllowedActions());
        }
    }

    private CliResponse MakeError(string error) =>
        new CliError(error, GetState(), GetSlotInfos(), _activeSlot, GetAllowedActions());

    private bool TryGetActiveSlot(out SlotState slot)
    {
        if (_activeSlot.HasValue && _slots.TryGetValue(_activeSlot.Value, out slot))
            return true;

        slot = default;
        return false;
    }

    private CliState GetState()
    {
        if (_activeSlot == null) return CliState.NoSlot;
        if (_openQueryId != null) return CliState.Streaming;
        return CliState.SlotReady;
    }

    private SlotInfo[] GetSlotInfos() =>
        Enumerable.Range(0, MaxSlots)
            .Select(i => _slots.TryGetValue(i, out SlotState s)
                ? new SlotInfo(i, s.FilePath)
                : new SlotInfo(i, null))
            .ToArray();

    private static SystemProcess StartWorkerProcess(int port)
    {
        string workerExe = Environment.GetEnvironmentVariable("PROLOG_WORKER_EXE")
            ?? Path.Combine(AppContext.BaseDirectory, "Prolog.NET.Worker");

        if (OperatingSystem.IsWindows())
            workerExe += ".exe";

        var psi = new SystemProcessStartInfo(workerExe, $"--port {port}")
        {
            UseShellExecute = false,
        };

        return SystemProcess.Start(psi)
            ?? throw new InvalidOperationException($"Failed to start worker process on port {port}");
    }
}

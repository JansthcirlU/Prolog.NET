namespace Prolog.NET.Actors;

/// <summary>The state the interactive CLI is in — drives which key bindings are shown.</summary>
public enum CliState { NoSlot, SlotReady, Streaming }

/// <summary>Describes one knowledge-base slot (one worker process).</summary>
/// <param name="Index">Zero-based slot index (0–3).</param>
/// <param name="FilePath">Path of the loaded file, or <see langword="null"/> if the slot is empty.</param>
public sealed record SlotInfo(int Index, string? FilePath);

// --- Inbound (PrologWorker → CliActor) ---

public sealed record LoadFileRequest(string Path);
public sealed record UnloadFileRequest(int Slot);
public sealed record SwitchSlotRequest(int Slot);
public sealed record QueryRequest(string Goal);
public sealed record NextSolutionRequest;
public sealed record CloseQueryRequest;
public sealed record HaltRequest;

// --- Outbound (CliActor → PrologWorker) ---
// Every response carries State, Slots[], and ActiveSlot so PrologWorker can fully redraw the UI.

public abstract record CliResponse(CliState State, SlotInfo[] Slots, int? ActiveSlot);
public sealed record CliOk(CliState State, SlotInfo[] Slots, int? ActiveSlot) : CliResponse(State, Slots, ActiveSlot);
public sealed record CliError(string Error, CliState State, SlotInfo[] Slots, int? ActiveSlot) : CliResponse(State, Slots, ActiveSlot);
public sealed record CliSolution(
    IReadOnlyDictionary<string, string> Variables,
    bool IsFinal,
    CliState State,
    SlotInfo[] Slots,
    int? ActiveSlot) : CliResponse(State, Slots, ActiveSlot);
public sealed record CliNoMoreSolutions(CliState State, SlotInfo[] Slots, int? ActiveSlot) : CliResponse(State, Slots, ActiveSlot);

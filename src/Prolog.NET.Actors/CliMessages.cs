namespace Prolog.NET.Actors;

/// <summary>The state the interactive CLI is in — drives which key bindings are shown.</summary>
public enum CliState { NoSlot, SlotReady, Streaming }

/// <summary>Describes one knowledge-base slot (one worker process).</summary>
/// <param name="Index">Zero-based slot index (0–3).</param>
/// <param name="FilePath">Path of the loaded file, or <see langword="null"/> if the slot is empty.</param>
public sealed record SlotInfo(int Index, string? FilePath);

/// <summary>Identifies a logical action the user can invoke.</summary>
public enum CliAction { LoadFile, UnloadFile, SwitchSlot, SubmitQuery, NextSolution, CloseQuery, Halt }

/// <summary>What additional input, if any, the action requires before it can be dispatched.</summary>
public enum ActionInput { None, FilePath, SlotNumber, QueryText }

/// <summary>One entry in the menu the frontend receives.</summary>
public sealed record AllowedAction(
    CliAction Action,
    string Label,
    char? KeyHint,
    ActionInput RequiredInput,
    string? InputPrompt = null);

// --- Inbound (PrologWorker → CliActor) ---

public sealed record GetStateRequest;
public sealed record LoadFileRequest(string Path);
public sealed record UnloadFileRequest(int Slot);
public sealed record SwitchSlotRequest(int Slot);
public sealed record QueryRequest(string Goal);
public sealed record NextSolutionRequest;
public sealed record CloseQueryRequest;
public sealed record HaltRequest;

// --- Outbound (CliActor → PrologWorker) ---
// Every response carries State, Slots[], ActiveSlot, and AllowedActions so PrologWorker can fully redraw the UI.

public abstract record CliResponse(CliState State, SlotInfo[] Slots, int? ActiveSlot, IReadOnlyList<AllowedAction> AllowedActions);
public sealed record CliOk(CliState State, SlotInfo[] Slots, int? ActiveSlot, IReadOnlyList<AllowedAction> AllowedActions) : CliResponse(State, Slots, ActiveSlot, AllowedActions);
public sealed record CliError(string Error, CliState State, SlotInfo[] Slots, int? ActiveSlot, IReadOnlyList<AllowedAction> AllowedActions) : CliResponse(State, Slots, ActiveSlot, AllowedActions);
public sealed record CliSolution(
    IReadOnlyDictionary<string, string> Variables,
    bool IsFinal,
    CliState State,
    SlotInfo[] Slots,
    int? ActiveSlot,
    IReadOnlyList<AllowedAction> AllowedActions) : CliResponse(State, Slots, ActiveSlot, AllowedActions);
public sealed record CliNoMoreSolutions(CliState State, SlotInfo[] Slots, int? ActiveSlot, IReadOnlyList<AllowedAction> AllowedActions) : CliResponse(State, Slots, ActiveSlot, AllowedActions);

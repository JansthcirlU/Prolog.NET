namespace Prolog.NET.Actors;

// --- Inbound messages ---

/// <summary>Instructs the <see cref="PrologActor"/> to load a Prolog source file.</summary>
public sealed record LoadFileMessage(string Path);

/// <summary>Instructs the <see cref="PrologActor"/> to execute a Prolog goal.</summary>
public sealed record CallMessage(string Goal);

/// <summary>Instructs the <see cref="PrologActor"/> to execute a Prolog goal and return all solutions.</summary>
public sealed record QueryMessage(string Goal);

// --- Lazy streaming inbound ---

/// <summary>Opens a lazy streaming query; the actor responds with <see cref="QueryOpenedMessage"/> or <see cref="QueryErrorMessage"/>.</summary>
public sealed record OpenQueryMessage(string Goal);

/// <summary>Requests the next solution for an open query.</summary>
public sealed record NextSolutionMessage(Guid QueryId);

/// <summary>Closes an open query without waiting for a reply.</summary>
public sealed record CloseQueryMessage(Guid QueryId);

// --- Responses ---

/// <summary>
/// Result of a <see cref="LoadFileMessage"/> or <see cref="CallMessage"/>.
/// </summary>
public sealed record CallResult(bool Success, string? ErrorMessage = null);

/// <summary>
/// Result of a <see cref="QueryMessage"/>. Each solution is a dictionary mapping
/// Prolog variable names (e.g. <c>"X"</c>) to their string representations.
/// </summary>
public sealed record QueryResult(
    IReadOnlyList<IReadOnlyDictionary<string, string>> Solutions,
    string? ErrorMessage = null);

// --- Lazy streaming responses ---

/// <summary>Sent by the actor after successfully opening a streaming query.</summary>
public sealed record QueryOpenedMessage(Guid QueryId);

/// <summary>One solution from a streaming query.</summary>
public sealed record SolutionMessage(IReadOnlyDictionary<string, string> Variables);

/// <summary>Sent by the actor when a streaming query has no more solutions.</summary>
public sealed record QueryEndMessage;

/// <summary>Sent by the actor when a streaming query encounters a Prolog-level error.</summary>
public sealed record QueryErrorMessage(string Error);

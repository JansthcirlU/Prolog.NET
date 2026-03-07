namespace Prolog.NET.Actors;

// --- Inbound messages ---

/// <summary>Instructs the <see cref="PrologActor"/> to load a Prolog source file.</summary>
public sealed record LoadFileMessage(string Path);

/// <summary>Instructs the <see cref="PrologActor"/> to execute a Prolog goal.</summary>
public sealed record CallMessage(string Goal);

/// <summary>Instructs the <see cref="PrologActor"/> to execute a Prolog goal and return all solutions.</summary>
public sealed record QueryMessage(string Goal);

// --- Lazy streaming inbound ---

/// <summary>Opens a lazy streaming query; the actor responds with an <see cref="OpenQueryResult"/>.</summary>
public sealed record OpenQueryMessage(string Goal);

/// <summary>Requests the next solution for an open query; the actor responds with a <see cref="NextSolutionResult"/>.</summary>
public sealed record NextSolutionMessage(Guid QueryId);

/// <summary>Closes an open query early (mid-stream cancellation). No reply is sent.</summary>
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

/// <summary>Discriminated union returned in response to <see cref="OpenQueryMessage"/>.</summary>
public abstract record OpenQueryResult;

/// <summary>The query was opened successfully. Use <see cref="QueryId"/> in subsequent <see cref="NextSolutionMessage"/> calls.</summary>
public sealed record QueryOpenedResult(Guid QueryId) : OpenQueryResult;

/// <summary>The query could not be opened (e.g. parse error or Prolog exception).</summary>
public sealed record OpenQueryFailedResult(string Error) : OpenQueryResult;

/// <summary>Discriminated union returned in response to <see cref="NextSolutionMessage"/>.</summary>
public abstract record NextSolutionResult;

/// <summary>
/// A solution was found and the query is still open.
/// Send another <see cref="NextSolutionMessage"/> to continue or <see cref="CloseQueryMessage"/> to cancel.
/// </summary>
public sealed record SolutionResult(IReadOnlyDictionary<string, string> Variables) : NextSolutionResult;

/// <summary>
/// The last solution (<c>PL_S_LAST</c>). The actor has already closed the query —
/// do <em>not</em> send <see cref="CloseQueryMessage"/>.
/// </summary>
public sealed record FinalSolutionResult(IReadOnlyDictionary<string, string> Variables) : NextSolutionResult;

/// <summary>
/// No more solutions (<c>PL_S_FALSE</c>). The actor has already closed the query —
/// do <em>not</em> send <see cref="CloseQueryMessage"/>.
/// </summary>
public sealed record NoMoreSolutionsResult : NextSolutionResult;

/// <summary>
/// A Prolog-level exception was raised. The actor has already closed the query —
/// do <em>not</em> send <see cref="CloseQueryMessage"/>.
/// </summary>
public sealed record QueryFailedResult(string Error) : NextSolutionResult;

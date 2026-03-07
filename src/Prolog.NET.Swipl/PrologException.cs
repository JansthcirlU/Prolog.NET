namespace Prolog.NET.Swipl;

/// <summary>
/// Represents an error that originates from the SWI-Prolog engine, such as a failed
/// initialization, a parse error in a goal string, or a Prolog exception thrown during
/// query execution.
/// </summary>
public sealed class PrologException : Exception
{
    /// <summary>
    /// The Prolog-level exception term as a string, if available.
    /// </summary>
    public string? PrologMessage { get; }

    public PrologException(string message, string? prologMessage = null)
        : base(prologMessage is not null ? $"{message}: {prologMessage}" : message)
    {
        PrologMessage = prologMessage;
    }
}

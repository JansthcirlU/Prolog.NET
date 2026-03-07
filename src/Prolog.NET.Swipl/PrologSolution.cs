namespace Prolog.NET.Swipl;

/// <summary>
/// Represents a single solution returned by a <see cref="PrologQuery"/>.
/// Provides access to the values of variables in the query's goal.
/// </summary>
/// <remarks>
/// Term references held by this solution are only valid while the originating
/// <see cref="PrologQuery"/> has not been disposed.
/// </remarks>
public sealed class PrologSolution
{
    // Maps Prolog variable name (e.g. "X") to its term_t handle (nuint).
    // The nuint values are opaque term references managed by the SWI-Prolog engine.
    private readonly IReadOnlyDictionary<string, nuint> _variables;

    // Pre-evaluated string representations of each variable, captured on the Prolog thread.
    private readonly IReadOnlyDictionary<string, string> _termStrings;

    internal PrologSolution(
        IReadOnlyDictionary<string, nuint> variables,
        IReadOnlyDictionary<string, string> termStrings)
    {
        _variables = variables;
        _termStrings = termStrings;
    }

    /// <summary>
    /// Returns the names of all variables available in this solution.
    /// </summary>
    public IReadOnlyCollection<string> VariableNames => _variables.Keys.ToArray();

    /// <summary>
    /// Returns the <see cref="PrologTerm"/> bound to the named variable.
    /// </summary>
    /// <param name="variableName">
    /// The Prolog variable name as it appears in the query (e.g. <c>"X"</c>).
    /// </param>
    /// <exception cref="KeyNotFoundException">
    /// Thrown if <paramref name="variableName"/> is not a variable in the query.
    /// </exception>
    public PrologTerm this[string variableName]
    {
        get
        {
            if (!_variables.TryGetValue(variableName, out nuint termRef))
            {
                throw new KeyNotFoundException(
                    $"Variable '{variableName}' is not present in this query. " +
                    $"Available variables: {string.Join(", ", _variables.Keys)}");
            }

            _ = _termStrings.TryGetValue(variableName, out string? cached);
            return new PrologTerm(termRef, cached);
        }
    }

    /// <summary>
    /// Attempts to get the <see cref="PrologTerm"/> for the named variable.
    /// </summary>
    public bool TryGetTerm(string variableName, out PrologTerm? term)
    {
        if (_variables.TryGetValue(variableName, out nuint termRef))
        {
            _ = _termStrings.TryGetValue(variableName, out string? cached);
            term = new PrologTerm(termRef, cached);
            return true;
        }

        term = null;
        return false;
    }
}

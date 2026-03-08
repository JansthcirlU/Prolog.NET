namespace Prolog.NET.Model;

/// <summary>
/// A Prolog database: an ordered collection of clauses (facts, rules, and directives)
/// that can be serialized to a <c>.pl</c> file.
/// </summary>
public sealed record PrologDatabase
{
    public IReadOnlyList<PrologClause> Clauses { get; }

    public PrologDatabase(IReadOnlyList<PrologClause> clauses)
    {
        ArgumentNullException.ThrowIfNull(clauses);
        Clauses = clauses;
    }
}

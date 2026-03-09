namespace Prolog.NET.Model;

public abstract record PrologDatabaseEntry;

public sealed record PrologFact(string Functor, IReadOnlyList<PrologTerm> Args) : PrologDatabaseEntry;

public sealed record PrologRuleClause(string Functor, IReadOnlyList<PrologTerm> Args, BodyGoal Body)
    : PrologDatabaseEntry;

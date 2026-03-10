namespace Prolog.NET.Model;

public abstract record PrologDatabaseEntry;

public sealed record PrologFact(string Functor, IReadOnlyList<PrologTerm> Args, string? Module = null) : PrologDatabaseEntry;

public sealed record PrologRuleClause(string Functor, IReadOnlyList<PrologTerm> Args, BodyGoal Body)
    : PrologDatabaseEntry;

public sealed record PrologMultifileDeclaration(string Functor, int Arity) : PrologDatabaseEntry;

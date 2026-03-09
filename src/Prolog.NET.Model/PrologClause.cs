namespace Prolog.NET.Model;

/// <summary>Base type for all Prolog clauses (facts, rules, and directives).</summary>
public abstract record PrologClause;

/// <summary>
/// A Prolog fact, e.g. <c>foo(a, b).</c> or <c>bar.</c>
/// The head must be an atom or a compound term.
/// </summary>
public sealed record PrologFact : PrologClause
{
    public PrologTerm Head { get; }

    public PrologFact(PrologTerm head)
    {
        ArgumentNullException.ThrowIfNull(head);
        if (head is not PrologAtom and not PrologCompoundTerm)
            throw new ArgumentException(
                "Fact head must be a PrologAtom or PrologCompoundTerm.",
                nameof(head));
        Head = head;
    }

    /// <summary>Creates a fact with a zero-arity head atom.</summary>
    public static PrologFact Of(string functor)
        => new(PrologAtom.Of(functor));

    /// <summary>Creates a fact with a compound head.</summary>
    public static PrologFact Of(string functor, params PrologTerm[] arguments)
        => new(PrologCompoundTerm.Of(functor, arguments));
}

/// <summary>
/// A Prolog rule, e.g. <c>grandparent(X, Z) :- parent(X, Y), parent(Y, Z).</c>
/// The head must be an atom or a compound term; the body must have at least one goal.
/// </summary>
public sealed record PrologRule : PrologClause
{
    public PrologTerm Head { get; }
    public IReadOnlyList<PrologTerm> Body { get; }

    public PrologRule(PrologTerm head, IReadOnlyList<PrologTerm> body)
    {
        ArgumentNullException.ThrowIfNull(head);
        ArgumentNullException.ThrowIfNull(body);
        if (head is not PrologAtom and not PrologCompoundTerm)
            throw new ArgumentException(
                "Rule head must be a PrologAtom or PrologCompoundTerm.",
                nameof(head));
        if (body.Count == 0)
            throw new ArgumentException(
                "Rule body must have at least one goal.",
                nameof(body));

        Head = head;
        Body = body;
    }

    /// <summary>
    /// Creates a rule with an explicit head argument list and body goal list (no lambdas).
    /// </summary>
    public static PrologRule Of(
        string functor,
        IReadOnlyList<PrologTerm> headArguments,
        IReadOnlyList<PrologTerm> body)
        => new(PrologCompoundTerm.Of(functor, [.. headArguments]), body);

    /// <summary>
    /// Creates a rule with a single named variable. The lambda receives the variable and
    /// returns <c>(headArgs, bodyGoals)</c> so the variable name is captured once.
    /// </summary>
    public static PrologRule Create(
        string functor,
        string varName,
        Func<PrologVariable, (IReadOnlyList<PrologTerm> head, IReadOnlyList<PrologTerm> body)> build)
    {
        var v = PrologVariable.Of(varName);
        var (headArgs, body) = build(v);
        return new(PrologCompoundTerm.Of(functor, [.. headArgs]), body);
    }
}

/// <summary>
/// A Prolog directive, e.g. <c>:- use_module(library(lists)).</c>
/// The goal must be an atom or a compound term.
/// </summary>
public sealed record PrologDirective : PrologClause
{
    public PrologTerm Goal { get; }

    public PrologDirective(PrologTerm goal)
    {
        ArgumentNullException.ThrowIfNull(goal);
        if (goal is not PrologAtom and not PrologCompoundTerm)
            throw new ArgumentException(
                "Directive goal must be a PrologAtom or PrologCompoundTerm.",
                nameof(goal));
        Goal = goal;
    }

    public static PrologDirective Of(string functor)
        => new(PrologAtom.Of(functor));

    public static PrologDirective Of(string functor, params PrologTerm[] arguments)
        => new(PrologCompoundTerm.Of(functor, arguments));
}

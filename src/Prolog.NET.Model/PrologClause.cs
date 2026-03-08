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
}

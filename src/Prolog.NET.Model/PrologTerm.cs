namespace Prolog.NET.Model;

/// <summary>Base type for all Prolog terms.</summary>
public abstract record PrologTerm;

/// <summary>A Prolog atom, e.g. <c>foo</c>, <c>'Hello World'</c>, <c>[]</c>.</summary>
public sealed record PrologAtom(AtomName Name) : PrologTerm;

/// <summary>A Prolog variable, e.g. <c>X</c>, <c>_G123</c>, <c>_</c>.</summary>
public sealed record PrologVariable(VariableName Name) : PrologTerm;

/// <summary>A Prolog integer, e.g. <c>42</c>.</summary>
public sealed record PrologInteger(long Value) : PrologTerm;

/// <summary>A Prolog floating-point number, e.g. <c>3.14</c>.</summary>
public sealed record PrologFloat(double Value) : PrologTerm;

/// <summary>
/// A Prolog compound term, e.g. <c>f(a, b)</c>. Must have at least one argument;
/// use <see cref="PrologAtom"/> for zero-arity terms.
/// </summary>
public sealed record PrologCompoundTerm : PrologTerm
{
    public AtomName Functor { get; }
    public IReadOnlyList<PrologTerm> Arguments { get; }

    public PrologCompoundTerm(AtomName functor, IReadOnlyList<PrologTerm> arguments)
    {
        ArgumentNullException.ThrowIfNull(arguments);
        if (arguments.Count == 0)
            throw new ArgumentException(
                "A compound term must have at least one argument. " +
                "Use PrologAtom for zero-arity terms.",
                nameof(arguments));

        Functor = functor;
        Arguments = arguments;
    }
}

/// <summary>
/// A Prolog list, e.g. <c>[a, b, c]</c> or <c>[]</c> (the empty list).
/// </summary>
public sealed record PrologList : PrologTerm
{
    public IReadOnlyList<PrologTerm> Elements { get; }

    public PrologList(IReadOnlyList<PrologTerm> elements)
    {
        ArgumentNullException.ThrowIfNull(elements);
        Elements = elements;
    }
}

/// <summary>
/// A Prolog predicate indicator, e.g. <c>foo/2</c>. Commonly used in module export lists.
/// </summary>
public sealed record PredicateIndicator : PrologTerm
{
    public AtomName Functor { get; }
    public int Arity { get; }

    public PredicateIndicator(AtomName functor, int arity)
    {
        if (arity < 0)
            throw new ArgumentOutOfRangeException(nameof(arity), "Arity must be non-negative.");
        Functor = functor;
        Arity = arity;
    }
}

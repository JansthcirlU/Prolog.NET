namespace Prolog.NET.Model;

public abstract record PrologTerm;

public sealed record PrologAtom : PrologTerm
{
    public string Name { get; init; }

    public PrologAtom(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentException("Atom name must not be empty.", nameof(name));
        }

        Name = name;
    }
}

public sealed record PrologInteger(long Value) : PrologTerm;

public sealed record PrologFloat(double Value) : PrologTerm;

public sealed record PrologString : PrologTerm
{
    public string Value { get; init; }

    public PrologString(string value)
    {
        if (value is null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        Value = value;
    }
}

public sealed record PrologVariable : PrologTerm
{
    public string Name { get; init; }

    public PrologVariable(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentException("Variable name must not be empty.", nameof(name));
        }

        if (!char.IsUpper(name[0]) && name[0] != '_')
        {
            throw new ArgumentException(
                "Variable name must start with an uppercase letter or underscore.", nameof(name));
        }

        foreach (char c in name.AsSpan(1))
        {
            if (!char.IsLetterOrDigit(c) && c != '_')
            {
                throw new ArgumentException(
                    "Variable name must contain only letters, digits, or underscores after the first character.",
                    nameof(name));
            }
        }
        Name = name;
    }
}

public sealed record PrologWildcard : PrologTerm
{
    public static readonly PrologWildcard Instance = new();

    private PrologWildcard() { }
}

public abstract record PrologList : PrologTerm
{
    public sealed record Nil : PrologList
    {
        public static readonly Nil Instance = new();

        private Nil() { }
    }
    public sealed record Cons(PrologTerm Head, PrologTerm Tail) : PrologList;
}

public sealed record PrologCompound : PrologTerm
{
    public string Functor { get; init; }
    public IReadOnlyList<PrologTerm> Args { get; init; }

    public PrologCompound(string functor, IReadOnlyList<PrologTerm> args)
    {
        if (string.IsNullOrEmpty(functor))
        {
            throw new ArgumentException("Functor name must not be empty.", nameof(functor));
        }

        if (args.Count < 1)
        {
            throw new ArgumentException("Compound term must have at least one argument.", nameof(args));
        }

        Functor = functor;
        Args = args;
    }
}

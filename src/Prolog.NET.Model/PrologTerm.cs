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

public sealed record PrologIntAtom(long Value) : PrologTerm;

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

namespace Prolog.NET.Model;

/// <summary>Wraps a ground C# value as a Prolog atom argument.</summary>
public record Atom<T>(T Value);

/// <summary>Wraps a C# type as a Prolog variable argument (unbound at query time).</summary>
public record Variable<T>(string Name);

/// <summary>
/// A typed query argument — either a concrete atom or an unbound variable.
/// </summary>
public abstract record QueryArgument<TArgument>
{
    public sealed record AtomArgument(Atom<TArgument> Atom) : QueryArgument<TArgument>;
    public sealed record VariableArgument(Variable<TArgument> Variable) : QueryArgument<TArgument>;
}

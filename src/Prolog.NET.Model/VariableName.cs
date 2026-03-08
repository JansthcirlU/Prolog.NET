using System.Text.RegularExpressions;

namespace Prolog.NET.Model;

/// <summary>
/// A validated Prolog variable name. Must start with an uppercase letter or underscore,
/// followed by zero or more letters, digits, or underscores (e.g. <c>X</c>, <c>_G123</c>, <c>_</c>).
/// </summary>
public sealed record VariableName
{
    private static readonly Regex ValidPattern =
        new(@"^[A-Z_][a-zA-Z0-9_]*$", RegexOptions.Compiled);

    public string Value { get; }

    public VariableName(string value)
    {
        ArgumentException.ThrowIfNullOrEmpty(value);
        if (!ValidPattern.IsMatch(value))
            throw new ArgumentException(
                $"'{value}' is not a valid Prolog variable name. " +
                "Variable names must start with an uppercase letter or underscore.",
                nameof(value));
        Value = value;
    }

    public override string ToString() => Value;
}

// Flexible, generic types to represent atoms or variables
public record Atom<T>(T Type);
public record Variable<T>(T Type);

// Represents an argument you can pass into a query
public abstract record QueryArgument<TArgument>
{
    public sealed record AtomArgument(Atom<TArgument> Atom) : QueryArgument<TArgument>;
    public sealed record VariableArgument(Variable<TArgument> Variable) : QueryArgument<TArgument>;
}

public static class Even
{
    // Types that can be 'even'
    public interface IEven;
    // even(0)
    public sealed record EvenValue : IEven
    {
        public int Value { get; }

        public EvenValue()
        {
            Value = 0;
        }
    }
    // even(N)
    public readonly record struct Any<U>(U Value) : IEven;
    // even(s(N))
    public readonly record struct S<O>(O Odd) : IEven where O : Odd.IOdd;
    
    // Represents a query on the 'even' rule
    public sealed record Query<TEven>(QueryArgument<TEven> Even) where TEven : struct, IEven;
    public static Query<Any<U>> QueryAny<U>(QueryArgument<Any<U>> even) => new(even);
    public static Query<S<O>> QueryS<O>(QueryArgument<S<O>> s) where O : Odd.IOdd => new(s);
}


public static class Odd
{
    // Types that can be 'odd'
    public interface IOdd;
    // odd(N)
    public readonly record struct Any<U>(U Value) : IOdd;
    // odd(s(N))
    public readonly record struct S<E>(E Even) : IOdd where E : Even.IEven;

    // Represents a query on the 'odd' rule
    public sealed record Query<TOdd>(QueryArgument<TOdd> Odd) where TOdd : struct, IOdd;
    public static Query<Any<U>> QueryOdd<U>(QueryArgument<Any<U>> odd) => new(odd);
    public static Query<S<E>> QueryS<E>(QueryArgument<S<E>> s) where E : Even.IEven => new(s);
}

# Modelling Prolog semantics using C#

## Relations and types

Let's use the Peano successor example:

```prolog
even(0).
even(s(N)) :- odd(N).
odd(s(N)) :- even(N).
```

In true Prolog fashion, this code snippet describes a simple set of relations between the concrete number `0`, the predicates `even`, `odd` and `s` and the variable `N`.
Prolog is relation-oriented and has no static type system, so trying to map it directly onto C# is going to be tricky.
Instead, I propose we analyze what the argument types can be for each relation, because that's where type definitions can be used.

### Types from the arguments' perspective

Consider the following C# code:

```cs
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
}

public static class Odd
{
    // Types that can be 'odd'
    public interface IOdd;
    // odd(N)
    public readonly record struct Any<U>(U Value) : IOdd;
    // odd(s(N))
    public readonly record struct S<E>(E Even) : IOdd where E : Even.IEven;
}

// No separate type to represent 's' because it's just a structural term, it doesn't correspond with a rule
```

Given each defined relationship, including nested ones inside compound definitions, the preceding snippet aims to represent each possible argument type that can go into each relationship.
This means that `Even` must correspond with the argument types that can go inside the `even` rule and that `Odd` must correspond with the argument types that can go inside the `odd` rule.

### Type-safe queries

Given the generated types above, it is now possible to define the possible query shapes for each rule:

```cs
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
    public interface IEven;
    public sealed record EvenValue : IEven
    {
        public int Value { get; }

        public EvenValue()
        {
            Value = 0;
        }
    }
    public readonly record struct Any<U>(U Value) : IEven;
    public readonly record struct S<O>(O Odd) : IEven where O : Odd.IOdd;
    
    // Represents a query on the 'even' rule
    public sealed record Query<TEven>(QueryArgument<TEven> Even) where TEven : struct, IEven;
    public static Query<Any<U>> QueryAny<U>(QueryArgument<Any<U>> even) => new(even);
    public static Query<S<O>> QueryS<O>(QueryArgument<S<O>> s) where O : Odd.IOdd => new(s);
}

public static class Odd
{
    public interface IOdd;
    public readonly record struct Any<U>(U Value) : IOdd;
    public readonly record struct S<E>(E Even) : IOdd where E : Even.IEven;

    // Represents a query on the 'odd' rule
    public sealed record Query<TOdd>(QueryArgument<TOdd> Odd) where TOdd : struct, IOdd;
    public static Query<Any<U>> QueryOdd<U>(QueryArgument<Any<U>> odd) => new(odd);
    public static Query<S<E>> QueryS<E>(QueryArgument<S<E>> s) where E : Even.IEven => new(s);
}
```

These additional types aren't meant to be used to recreate a type-safe Prolog implementation, but having a type to represent the concept of a query will allow us to write more complex rules in a predictable manner, avoiding easy-to-miss type mismatches and structural incompatibilities.

### Code generation

All of this logic of course hinges on a type hierarchy that allows code generation appropriately.

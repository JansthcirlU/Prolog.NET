# Modelling Prolog semantics using C#

## Relations and types (v1 approach)

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
This means that `IEven` must correspond with the argument types that can go inside the `even` rule and that `IOdd` must correspond with the argument types that can go inside the `odd` rule.

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
    public static Query<Any<U>> QueryAny<U>(QueryArgument<Any<U>> odd) => new(odd);
    public static Query<S<E>> QueryS<E>(QueryArgument<S<E>> s) where E : Even.IEven => new(s);
}
```

These additional types aren't meant to be used to recreate a type-safe Prolog implementation, but having a type to represent the concept of a query will allow us to write more complex rules in a predictable manner, avoiding easy-to-miss type mismatches and structural incompatibilities.

For more complex rule hierarchies, a query could contain nested variables, something akin to `f(a, g(X), Y)` where only `X` and `Y` are variables without needing to write `QueryArgument<F>` or something. Initially, I thought of using the `out` parameter, but I think this approach is more scalable.

### Code generation

All of this logic of course hinges on a type hierarchy that allows code generation, for example via source generators, to create these additional relationship argument types which can then be re-used to create additional queries or even new rules. The current types work to represent the different semantic constructs in Prolog, but it might not map nicely onto the "relations" representation. Having a more relation-argument-oriented type hierarchy might make it easier to gather relations with the same name and combine all the different definitions together to generate the output code for the args.

## A more streamlined type encoding (v2 approach)

The first approach focused heavily on supporting queries, but it ultimately made a distinction between rules and queries.
However, my mental model when writing Prolog is that rules use queries to relate matching atoms, which is why I believe the concepts should be more closely related.
For example, with the even/odd example, the rule `even(s(N)) :- odd(N)` relates the rules `even` and `odd` through the existence of some `N` that may also appear in a functor `s` somewhere.
In my mental model, I would read the rule as follows: "Find me an `N` that satisfies both `odd` and exists somewhere across the whole database inside some functor `s`."

So for this v2 approach, I propose to make queries first-class citizens when building rules, and as I envision it, this approach would need a complete revamp of the source code.

### Rework: dedicated types to distinguish facts

Rather than defining facts by its syntactical structure (atom name, value, etc.), you could use type hierarchies or partial types to group together common facts.
Consider the following family tree example in Prolog:

```prolog
% Facts
male(homer).
male(bart).
female(lisa).
parent(homer, bart).
parent(homer, lisa).
```

#### Facts as implementations of rules?

Ignoring the rules for now, we can leverage C# types as a way to distinguish Prolog facts simply by using the fact that the C# type system treats different types as distinct from each other.

```cs
[PrologFact("homer")]
[PrologFact("bart")]
public record Male;

[PrologFact("lisa")]
public record Female;
```

However, this approach would fall apart for facts with compound expressions, such as:

```prolog
% Compound
lives_at(homer, address(street(evergreen_terrace, 742), springfield)).
```

The first approach does not support type-safe compound expressions, when in this case it would make a lot more sense to translate it into C# like this:

```cs
// Type-safe structural functors
public readonly record struct Street<TArg1, TArg2>(AtomOrVariable<TArg1> Arg1, AtomOrVariable<TArg2> Arg2); // Arg1 and Arg2 are names that can be fixed with C# name hint attributes
public readonly record struct Address<TStreetArg1, TStreetArg2, TArg2>(AtomOrVariable<Street<TStreetArg1, TStreetArg2>> Street, AtomOrVariable<TArg2> Arg2); // Same for Arg2

// This does not compile! Attributes do not support arbitrary type arguments!
[PrologFact<string, Address<string, int, string>>(
    "homer",
    new Address<string>(
        Street: AtomOrVariable<Street<string, int>>.Atom(new Street("evergreen_terrace", 742)),
        Arg2: AtomOrVariable<string>.Atom("springfield")))]
public record LivesAt;
```

#### Relations as first-class citizens

Since C# 11, however, it's possible to write generic attributes, like so:

```cs
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
public abstract class PrologRelationAttribute : System.Attribute
{
}
public class PrologRelationAttribute<T1> : PrologRelationAttribute { /* ... */ }
public class PrologRelationAttribute<T1, T2> : PrologRelationAttribute { /* ... */ }
public class PrologRelationAttribute<T1, T2, T3> : PrologRelationAttribute { /* ... */ }
// And so on

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
public class PrologRelationNameAttribute(string Name) : System.Attribute;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
public record FunctorAttribute<T1>(string Name);
public record FunctorAttribute<T1, T2>(string Name);
public record FunctorAttribute<T1, T2, T3>(string Name);
// And so on

[PrologFunctor<PrologArgument, PrologArgument>("street")]
public record Street : IFunctor;

[PrologFunctor<Street, PrologArgument>("address")]
public record Address : IFunctor;

[PrologRelationName("lives_at")]
[PrologRelation<PrologArgument, Address>] // Functor type is to be decomposed to allow for matching subqueries like `address(street(X, 742), springfield)`
// Unlike with functors, additional PrologRelation declarations are allowed to accommodate different shapes
public record LivesAt : IRelation;
```

This unifies the concept of facts, relations and queries all into a single API, which has the following consequences:
1. the *user* must declare relations and functors before they can even start writing facts, which can be rather unintuitive but type safe
2. the *source generator* must now do a lot more of the heavy lifting to generate code to:
  - create facts
  - compose relationships
  - query relationships
3. we must design the necessary types abstractions that can be generated by the source generator
4. we must design a DSL on top of those types to compose rules, facts and queries, which can then be aggregated into a prolog database to be serialized into a `.pl` file

The flexibility of prototyping quickly in Prolog is sacrificed in favor of more consistency across the knowledge base, where relations are semantically significant by virtue of the type system and structurally robust by virtue of the fact that the developer must declare functor shapes as attributes before using them. It asks for a little bit more developer discipline, but ultimately I believe that this is more structured and more readable than hand-written Prolog.

#### What the source generator needs

The generator reads `[PrologRelationName]`, `[PrologRelation<...>]`, and `[PrologFunctor<...>]` attributes from annotated `IRelation`/`IFunctor` types.
It extracts the Prolog name, the argument shape(s) per relation (one per `[PrologRelation<...>]` attribute), and the argument shape per functor.
It must resolve cross-references between types (e.g. `[PrologRelation<S>]` references the S functor type declared elsewhere).

#### What should be generated

For each `IRelation` type (e.g. `Even`), emit static factory methods directly on the relation type:
- `Fact(...)` — one overload per declared shape, returning a `PrologFact`
- `Query(...)` — one overload per shape, covering all atom/variable combinations per argument position; arity is enforced per-relation so passing the wrong number of arguments is a compile error
- `Rule()` — returns a `RuleBuilder` scoped to this relation

For each `IFunctor` type (e.g. `S`), emit a static `Of(...)` factory matching the declared argument shape. Each argument position accepts either a `PrologVariable` or a `PrologAtom` via overloads (or a shared `IPrologTerm` base).

`Variables(...)` overloads (1–N variables) are a fixed mechanical set, suitable for source generation or copy-paste.

#### What the DSL needs

- `RuleBuilder` — produced by `Xxx.Rule()`. Exposes `.AddDefinition(Action<DefinitionBuilder> build)`.
- `DefinitionBuilder` — represents one clause. Exposes:
  - `.Variables(string name1, ..., Func<PrologVariable, ..., DefinitionBuilder, DefinitionBuilder> build)` — flat variable declaration, one overload per variable count
  - `.Arguments(...)` — head argument positions for this definition
  - `.Body(...)` — body goals, combinable with `.And()`/`.Or()` extensions; accepts `true` for unconditional facts
- Body goals are queries returned by `Xxx.Query(...)` — arity enforced at compile time per relation.

#### Rough sketch of the intended implementation

Revisiting the odd/even example:

```prolog
even(0).
even(s(N)) :- odd(N).
odd(s(N)) :- even(N).
```

This tells me, the developer, that `even` and `odd` can both contain an arbitrary atom or variable (like `0` or `N`), but also an `s/1` functor.
I would therefore have to declare the functor and relations like this:

```cs
[PrologFunctor("s")]
public record S : IFunctor; // Interface might not be necessary in the finalized API, I'm just using it here as a way to double-check that a functor attribute decorates a functor type

[PrologRelationName("even")]
[PrologRelation<PrologArgument>] // Represents the "any" case 'even(0)' or 'even(N)'
[PrologRelation<S>] // Represents the "s" case 'even(s(N))' or 'even(s(some_atom))' even though there is no known atom in that position
public record Even : IRelation; // Again, interface might not be needed in the final API here either

// Same deal for odd
[PrologRelationName("odd")]
[PrologRelation<PrologArgument>]
[PrologRelation<S>]
public record Odd : IRelation;
```

As the solution builds, the source generator will run to generate static factory methods on each relation and functor type to instantiate facts and queries based on the known relation shapes.
Furthermore, this assumes that there is a type hierarchy that can represent atoms, variables, facts, queries and rules.
Finally, to actually represent the even/odd rules in C#, I would use the generated types like this:

Atoms intended for reuse are declared as plain C# variables before `Database.Create(...)` — C# scoping handles reuse naturally.
Logical variables are declared inside `.Variables(...)` because they are scoped to a single rule definition and have no meaning outside it.

```cs
using Prolog.Model.Generated;

// Atoms are ground values: declare them as C# variables for reuse
var zero = Prolog.Atom.CreateInt(0);

PrologDatabase evenOdd = Prolog.Database.Create([
    Even.Fact(zero),
    Even.Rule()
        .AddDefinition(rule => rule                          // even(s(N)) :- odd(N).
            .Variables("N", (N, r) => r
                .Arguments(S.Of(N))
                .Body(Odd.Query(N)))),
    Odd.Rule()
        .AddDefinition(rule => rule                          // odd(s(N)) :- even(N).
            .Variables("N", (N, r) => r
                .Arguments(S.Of(N))
                .Body(Even.Query(N)))),
]);
```

For rules where an atom appears in multiple positions (head and body), declare it once outside and reference it freely:

```cs
var bart = Prolog.Atom.Create("bart");

PrologDatabase family = Prolog.Database.Create([
    FatherOfBart.Rule()
        .AddDefinition(rule => rule                          // father_of_bart(bart, Father) :- male(Father), parent(Father, bart).
            .Variables("Father", (Father, r) => r
                .Arguments(bart, Father)
                .Body(Male.Query(Father).And(Parent.Query(Father, bart))))),
]);
```

To build more complex queries inside a rule body, the DSL should include `And()` and `Or()` extensions to align with Prolog semantics.

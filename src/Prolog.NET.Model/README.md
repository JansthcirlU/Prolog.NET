# Prolog.NET.Model

Schema-first DSL for constructing typed Prolog databases in C#. Declare functor and relation shapes using attributes on `partial` records; the companion source generator emits strongly-typed factory methods at compile time; `PrologSerializer` converts the result to `.pl` source.

## Generator

`Prolog.NET.Model` is designed to be used with `Prolog.NET.Model.Generator`, a Roslyn incremental analyzer that must be referenced alongside this package:

```xml
<ProjectReference Include="..\Prolog.NET.Model.Generator\Prolog.NET.Model.Generator.csproj"
                  OutputItemType="Analyzer"
                  ReferenceOutputAssembly="false" />
```

If a decorated type is not declared `partial`, the generator emits diagnostic **`PNET001`** (error). Set `<EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>` in your `.csproj` to inspect generated files under `obj/<Configuration>/<TFM>/generated/`.

## Usage

### Declaring Relations

A relation type is a `partial record` decorated with `PrologRelationName` and one or more `PrologRelation<T…>` attributes. Place it inside a `static partial class` to group related declarations:

```csharp
public static partial class PersonModule
{
    [PrologModule("person")]
    [PrologRelationName("person")]
    [PrologRelation<Name>]
    public partial record Person : IRelation;
}
```

- **`[PrologModule("person")]`** — assigns the type to a Prolog module; generated `Query()` calls include the `module:functor(…)` qualifier automatically.
- **`[PrologRelationName("person")]`** — sets the Prolog functor name.
- **`[PrologRelation<T…>]`** — one attribute per arity; the number of type parameters determines how many arguments `Fact(…)` and `Query(…)` accept.

Use `PrologArgument` as the wildcard type for each position (a marker type; never instantiated). Multiple `PrologRelation` attributes on the same type register multiple arities.

### Declaring Functors

A functor type is a `partial record` decorated with `PrologFunctor<T…>`:

```csharp
[PrologFunctor<PrologArgument, PrologArgument>("name")]
public partial record Name : IFunctor;
```

The generator emits `Name.Of(PrologTerm, PrologTerm)` returning a `PrologCompound`. For example:

```csharp
Name.Of(PrologDSL.Atom.Create("alice"), PrologDSL.Atom.Create("smith"))
// → PrologCompound("name", […])  →  name(alice, smith)
```

A type can carry both relation and functor attributes simultaneously, letting it act as both a top-level relation and a reusable compound term:

```csharp
[PrologModule("addresses")]
[PrologRelationName("address")]
[PrologRelation<PrologArgument, PrologArgument, PrologArgument>]
[PrologFunctor<PrologArgument, PrologArgument, PrologArgument>("address")]
public partial record Address : IRelation, IFunctor;
```

### Generated Methods Summary

| Attribute | Generated method | Returns |
|-----------|-----------------|---------|
| `PrologRelationAttribute<T…>` | `Fact(…)` | `PrologFact` |
| `PrologRelationAttribute<T…>` | `Query(…)` | `BodyGoal` |
| `PrologRelationNameAttribute` | `Rule()` | `RuleBuilder` |
| `PrologRelationNameAttribute` | `Multifile()` | `PrologDatabaseItem` |
| `PrologFunctorAttribute<T…>` | `Of(…)` | `PrologCompound` |

### Three-Module Example

Declarations for three related modules:

```csharp
public static partial class PersonModule
{
    // 'name' compound: used inside person facts as name(first, last)
    [PrologFunctor<PrologArgument, PrologArgument>("name")]
    public partial record Name : IFunctor;

    [PrologModule("person")]
    [PrologRelationName("person")]
    [PrologRelation<Name>]
    public partial record Person : IRelation;
}

public static partial class AddressesModule
{
    // 'address' is both a top-level relation and a reusable compound term
    [PrologModule("addresses")]
    [PrologRelationName("address")]
    [PrologRelation<PrologArgument, PrologArgument, PrologArgument>]    // address(street, number, city)
    [PrologFunctor<PrologArgument, PrologArgument, PrologArgument>("address")]
    public partial record Address : IRelation, IFunctor;
}

public static partial class ResidentsModule
{
    [PrologModule("residents")]
    [PrologRelationName("resident")]
    [PrologRelation<Name, Address>]
    public partial record Resident : IRelation;

    [PrologModule("residents")]
    [PrologRelationName("live_together")]
    [PrologRelation<Name, Name>]
    public partial record LiveTogether : IRelation;
}
```

Building the databases:

```csharp
var alice       = PrologDSL.Atom.Create("alice");
var smith       = PrologDSL.Atom.Create("smith");
var bob         = PrologDSL.Atom.Create("bob");
var jones       = PrologDSL.Atom.Create("jones");
var elm         = PrologDSL.Atom.Create("elm");
var springfield = PrologDSL.Atom.Create("springfield");
var n42         = PrologDSL.Atom.CreateInt(42);

PrologModule person = PrologDSL.Module.Create("person", [
    PersonModule.Person.Fact(PersonModule.Name.Of(alice, smith)),   // person(name(alice, smith)).
    PersonModule.Person.Fact(PersonModule.Name.Of(bob, jones)),     // person(name(bob, jones)).
]);

PrologModule addresses = PrologDSL.Module.Create("addresses", [
    AddressesModule.Address.Fact(elm, n42, springfield),            // address(elm, 42, springfield).
]);

PrologModule residents = PrologDSL.Module.Create("residents", [
    // resident(name(alice, smith), address(elm, 42, springfield)).
    ResidentsModule.Resident.Fact(
        PersonModule.Name.Of(alice, smith),
        AddressesModule.Address.Of(elm, n42, springfield)),
    // resident(name(bob, jones), address(elm, 42, springfield)).
    ResidentsModule.Resident.Fact(
        PersonModule.Name.Of(bob, jones),
        AddressesModule.Address.Of(elm, n42, springfield)),
    // live_together(P1, P2) :-
    //     person:person(P1), person:person(P2),
    //     resident(P1, A), resident(P2, A).
    ResidentsModule.LiveTogether.Rule()
        .AddDefinition(rule => rule
            .Variables("P1", "P2", "A", (p1, p2, a, r) => r
                .Arguments(p1, p2)
                .Body(PersonModule.Person.Query(p1)              // person:person(P1)
                    .And(PersonModule.Person.Query(p2))          // person:person(P2)
                    .And(ResidentsModule.Resident.Query(p1, a))  // resident(P1, A)
                    .And(ResidentsModule.Resident.Query(p2, a))))),
]);
```

### Serializing

```csharp
string personSource    = PrologSerializer.Serialize(person);
string addressSource   = PrologSerializer.Serialize(addresses);
string residentsSource = PrologSerializer.Serialize(residents);
```

`use_module` directives are emitted automatically for any cross-module calls detected in rule bodies. For example, `residentsSource` produces:

```prolog
:- module(residents, [resident/2, live_together/2]).

:- use_module(person).

resident(name(alice, smith), address(elm, 42, springfield)).
resident(name(bob, jones), address(elm, 42, springfield)).
live_together(P1, P2) :-
    person:person(P1),
    person:person(P2),
    resident(P1, A),
    resident(P2, A).
```

---

## Tests

End-to-end serialization tests for this package live in `tests/Prolog.NET.Model.Tests/`. The `ReadmeTests` class builds the three-module example from this README and asserts the exact serialized output for each module.

---

## Appendix: Type Reference

### DSL Entry Point

| Type | Description |
|------|-------------|
| `PrologDSL` | Static entry point; `PrologDSL.Atom.Create`, `PrologDSL.Atom.CreateInt`, `PrologDSL.Database.Create`, `PrologDSL.Module.Create` |

### Database Types

| Type | Description |
|------|-------------|
| `PrologDatabase` | Ordered list of `PrologDatabaseEntry` records |
| `PrologModule` | Named wrapper around a `PrologDatabase`; serializes with a module declaration and auto-emitted `use_module` directives |

### Term Types

| Type | Description |
|------|-------------|
| `PrologAtom` | A named atom |
| `PrologIntAtom` | An integer atom (`long`) |
| `PrologVariable` | A Prolog variable |
| `PrologCompound` | A compound term with a functor and arguments |

### Body Goal Types

| Type | Description |
|------|-------------|
| `Call` | A goal call, optionally qualified with a module |
| `Conjunction` | `Left , Right` |
| `Disjunction` | `Left ; Right` |
| `True` | The `true` goal |

Goals support `.And()` and `.Or()` builder methods.

### Attributes

| Attribute | Description |
|-----------|-------------|
| `PrologModuleAttribute(string)` | Assigns the type to a Prolog module; qualifies generated `Query()` calls |
| `PrologRelationNameAttribute(string)` | Names the relation and marks the type for the relation pipeline |
| `PrologRelationAttribute<T1..T4>` | Declares an arity-N overload for `Fact` / `Query` / `Rule()` / `Multifile()` |
| `PrologFunctorAttribute<T1..T4>` | Declares an arity-N `Of(…)` factory method returning `PrologCompound` |

namespace Prolog.NET.Model.Tests;

public static partial class PersonModule
{
    [PrologFunctor<PrologArgument, PrologArgument>("name")]
    public partial record Name : IFunctor;

    [PrologModule("person")]
    [PrologRelationName("person")]
    [PrologRelation<Name>]
    public partial record Person : IRelation;
}

public static partial class AddressesModule
{
    [PrologModule("addresses")]
    [PrologRelationName("address")]
    [PrologRelation<PrologArgument, PrologArgument, PrologArgument>]
    [PrologFunctor<PrologArgument, PrologArgument, PrologArgument>("address")]
    public partial record Address : IRelation, IFunctor;
}

public static partial class ResidentsModule
{
    [PrologModule("residents")]
    [PrologRelationName("resident")]
    [PrologRelation<PersonModule.Name, AddressesModule.Address>]
    public partial record Resident : IRelation;

    [PrologModule("residents")]
    [PrologRelationName("live_together")]
    [PrologRelation<PersonModule.Name, PersonModule.Name>]
    public partial record LiveTogether : IRelation;
}

public class ReadmeTests
{
    private static readonly PrologAtom Alice = PrologDSL.Atom.Create("alice");
    private static readonly PrologAtom Smith = PrologDSL.Atom.Create("smith");
    private static readonly PrologAtom Bob = PrologDSL.Atom.Create("bob");
    private static readonly PrologAtom Jones = PrologDSL.Atom.Create("jones");
    private static readonly PrologAtom Elm = PrologDSL.Atom.Create("elm");
    private static readonly PrologAtom Springfield = PrologDSL.Atom.Create("springfield");
    private static readonly PrologInteger N42 = PrologDSL.Atom.CreateInt(42);

    [Fact]
    public void PersonModule_SerializesCorrectly()
    {
        PrologModule person = PrologDSL.Module.Create("person", [
            PersonModule.Person.Fact(PersonModule.Name.Of(Alice, Smith)),
            PersonModule.Person.Fact(PersonModule.Name.Of(Bob, Jones)),
        ]);

        string actual = PrologSerializer.Serialize(person);

        string expected =
            """
            :- module(person, [person/1]).

            person(name(alice, smith)).
            person(name(bob, jones)).

            """;

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void AddressesModule_SerializesCorrectly()
    {
        PrologModule addresses = PrologDSL.Module.Create("addresses", [
            AddressesModule.Address.Fact(Elm, N42, Springfield),
        ]);

        string actual = PrologSerializer.Serialize(addresses);

        string expected =
            """
            :- module(addresses, [address/3]).

            address(elm, 42, springfield).

            """;

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ResidentsModule_SerializesCorrectly()
    {
        PrologModule residents = PrologDSL.Module.Create("residents", [
            ResidentsModule.Resident.Fact(
                PersonModule.Name.Of(Alice, Smith),
                AddressesModule.Address.Of(Elm, N42, Springfield)),
            ResidentsModule.Resident.Fact(
                PersonModule.Name.Of(Bob, Jones),
                AddressesModule.Address.Of(Elm, N42, Springfield)),
            ResidentsModule.LiveTogether.Rule()
                .AddDefinition(rule => rule
                    .Variables("P1", "P2", "A", (p1, p2, a, r) => r
                        .Arguments(p1, p2)
                        .Body(PersonModule.Person.Query(p1)
                            .And(PersonModule.Person.Query(p2))
                            .And(ResidentsModule.Resident.Query(p1, a))
                            .And(ResidentsModule.Resident.Query(p2, a))))),
        ]);

        string actual = PrologSerializer.Serialize(residents);

        string expected =
            """
            :- module(residents, [resident/2, live_together/2]).

            :- use_module(person).

            resident(name(alice, smith), address(elm, 42, springfield)).
            resident(name(bob, jones), address(elm, 42, springfield)).
            live_together(P1, P2) :-
                person:person(P1),
                person:person(P2),
                resident(P1, A),
                resident(P2, A).

            """;

        Assert.Equal(expected, actual);
    }
}

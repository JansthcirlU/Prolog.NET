using Prolog.NET.Designer;
using Prolog.NET.Model;

PrologIntAtom zero = PrologDSL.Atom.CreateInt(0);
PrologAtom homer = PrologDSL.Atom.Create("homer");
PrologAtom marge = PrologDSL.Atom.Create("marge");
PrologAtom bart = PrologDSL.Atom.Create("bart");
PrologAtom lisa = PrologDSL.Atom.Create("lisa");

PrologDatabase family = PrologDSL.Database.Create([
    Male.Fact(homer),
    Male.Fact(bart),
    Female.Fact(marge),
    Female.Fact(lisa),
    Parent.Fact(homer, bart),
    Parent.Fact(homer, lisa),
    Parent.Fact(marge, bart),
    Parent.Fact(marge, lisa),
    Father.Rule()
        .AddDefinition(rule => rule
            .Variables("Father", "Child", (f, c, r) => r
                .Arguments(f, c)
                .Body(Parent.Query(f, c).And(Male.Query(f))))),
    Even.Fact(zero),
    Even.Rule()
        .AddDefinition(rule => rule
            .Variables("N", (n, r) => r
                .Arguments(S.Of(n))
                .Body(Odd.Query(n)))),
    Odd.Rule()
        .AddDefinition(rule => rule
            .Variables("N", (n, r) => r
                .Arguments(S.Of(n))
                .Body(Even.Query(n)))),
]);

Console.Write(PrologSerializer.Serialize(family));

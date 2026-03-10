using Prolog.NET.Designer.Modules;
using Prolog.NET.Model;

PrologIntAtom zero = PrologDSL.Atom.CreateInt(0);
PrologAtom homer = PrologDSL.Atom.Create("homer");
PrologAtom marge = PrologDSL.Atom.Create("marge");
PrologAtom bart = PrologDSL.Atom.Create("bart");
PrologAtom lisa = PrologDSL.Atom.Create("lisa");

PrologModule family = PrologDSL.Module.Create("family", [
    FamilyModule.Male.Fact(homer),
    FamilyModule.Male.Fact(bart),
    FamilyModule.Female.Fact(marge),
    FamilyModule.Female.Fact(lisa),
    FamilyModule.Parent.Fact(homer, bart),
    FamilyModule.Parent.Fact(homer, lisa),
    FamilyModule.Parent.Fact(marge, bart),
    FamilyModule.Parent.Fact(marge, lisa),
    FamilyModule.Father.Rule()
        .AddDefinition(rule => rule
            .Variables("Father", "Child", (f, c, r) => r
                .Arguments(f, c)
                .Body(FamilyModule.Parent.Query(f, c).And(FamilyModule.Male.Query(f))))),
]);

PrologModule peano = PrologDSL.Module.Create("peano", [
    PeanoModule.Even.Fact(zero),
    PeanoModule.Even.Rule()
        .AddDefinition(rule => rule
            .Variables("N", (n, r) => r
                .Arguments(PeanoModule.S.Of(n))
                .Body(PeanoModule.Odd.Query(n)))),
    PeanoModule.Odd.Rule()
        .AddDefinition(rule => rule
            .Variables("N", (n, r) => r
                .Arguments(PeanoModule.S.Of(n))
                .Body(PeanoModule.Even.Query(n)))),
]);

PrologModule genealogy = PrologDSL.Module.Create("genealogy", [
    GenealogyModule.Grandfather.Rule()
        .AddDefinition(rule => rule
            .Variables("GF", "P", "GC", (gf, p, gc, r) => r
                .Arguments(gf, gc)
                .Body(FamilyModule.Father.Query(gf, p).And(FamilyModule.Father.Query(p, gc))))),
]);

Console.Write(PrologSerializer.Serialize(family));
Console.WriteLine();
Console.Write(PrologSerializer.Serialize(genealogy));
Console.WriteLine();
Console.Write(PrologSerializer.Serialize(peano));

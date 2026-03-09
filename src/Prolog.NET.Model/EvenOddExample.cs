using Prolog.NET.Model;

namespace Prolog.NET.Model.Examples;

[PrologFunctor<PrologArgument>("s")]
public partial record S : IFunctor;

[PrologRelationName("even")]
[PrologRelation<PrologArgument>]
[PrologRelation<S>]
public partial record Even : IRelation;

[PrologRelationName("odd")]
[PrologRelation<PrologArgument>]
[PrologRelation<S>]
public partial record Odd : IRelation;

public static class EvenOddExample
{
    public static PrologDatabase Build()
    {
        PrologIntAtom zero = PrologDSL.Atom.CreateInt(0);

        return PrologDSL.Database.Create([
            Even.Fact(zero),
            Even.Rule()
                .AddDefinition(rule => rule
                    .Variables("N", (N, r) => r
                        .Arguments(S.Of(N))
                        .Body(Odd.Query(N)))),
            Odd.Rule()
                .AddDefinition(rule => rule
                    .Variables("N", (N, r) => r
                        .Arguments(S.Of(N))
                        .Body(Even.Query(N)))),
        ]);
    }
}

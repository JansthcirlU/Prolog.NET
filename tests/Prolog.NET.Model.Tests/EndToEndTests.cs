using Prolog.NET.Model;

namespace Prolog.NET.Model.Tests;

public static partial class MathUtils
{
    [PrologRelationName("greater")]
    [PrologRelation<PrologArgument, PrologArgument>]
    public partial record Greater : IRelation;

    [PrologRelationName("max")]
    [PrologRelation<PrologArgument, PrologArgument, PrologArgument>]
    public partial record Max : IRelation;

    [PrologRelationName("non_negative")]
    [PrologRelation<PrologArgument>]
    public partial record NonNegative : IRelation;

    [PrologRelationName("arith_equal")]
    [PrologRelation<PrologArgument, PrologArgument>]
    public partial record ArithEqual : IRelation;

    [PrologRelationName("conditional_max")]
    [PrologRelation<PrologArgument, PrologArgument, PrologArgument>]
    public partial record ConditionalMax : IRelation;
}

public static partial class TypeClassifier
{
    [PrologRelationName("classify")]
    [PrologRelation<PrologArgument, PrologArgument>]
    public partial record Classify : IRelation;

    [PrologRelationName("not_identical")]
    [PrologRelation<PrologArgument, PrologArgument>]
    public partial record NotIdentical : IRelation;

    [PrologRelationName("comes_before")]
    [PrologRelation<PrologArgument, PrologArgument>]
    public partial record ComesBefore : IRelation;

    [PrologRelationName("unique_solution")]
    [PrologRelation<PrologArgument>]
    public partial record UniqueSolution : IRelation;
}

public class EndToEndTests
{
    [Fact]
    public void ArithmeticAndControl_EndToEnd()
    {
        PrologDatabase db = PrologDSL.Database.Create([
            MathUtils.Greater.Rule()
                .AddDefinition(rule => rule
                    .Variables("X", "Y", (x, y, r) => r
                        .Arguments(x, y)
                        .Body(PrologDSL.Goals.ArithGt(x, y)))),

            MathUtils.Max.Rule()
                .AddDefinition(rule => rule
                    .Variables("X", "Y", (x, y, r) => r
                        .Arguments(x, y, x)
                        .Body(PrologDSL.Goals.ArithGeq(x, y).And(PrologDSL.Goals.Cut)))),

            MathUtils.Max.Fact(PrologDSL.Wildcard, new PrologVariable("Y"), new PrologVariable("Y")),

            MathUtils.NonNegative.Rule()
                .AddDefinition(rule => rule
                    .Variables("X", (x, r) => r
                        .Arguments(x)
                        .Body(PrologDSL.Goals.Not(PrologDSL.Goals.ArithLt(x, new PrologIntAtom(0)))))),

            MathUtils.ArithEqual.Rule()
                .AddDefinition(rule => rule
                    .Variables("X", "Y", (x, y, r) => r
                        .Arguments(x, y)
                        .Body(PrologDSL.Goals.ArithEq(x, y)))),

            MathUtils.ConditionalMax.Rule()
                .AddDefinition(rule => rule
                    .Variables("X", "Y", (x, y, r) => r
                        .Arguments(x, y, x)
                        .Body(PrologDSL.Goals.ArithGeq(x, y).IfThen(new True()).Or(new Fail())))),

            MathUtils.ConditionalMax.Fact(PrologDSL.Wildcard, new PrologVariable("Y"), new PrologVariable("Y")),
        ]);

        string actual = PrologSerializer.Serialize(db);

        string expected = """
            greater(X, Y) :-
                X > Y.
            max(X, Y, X) :-
                X >= Y,
                !.
            max(_, Y, Y).
            non_negative(X) :-
                \+(X < 0).
            arith_equal(X, Y) :-
                X =:= Y.
            conditional_max(X, Y, X) :-
                ((X >= Y -> true)
                ; fail).
            conditional_max(_, Y, Y).

            """;

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TypeCheckAndControl_EndToEnd()
    {
        PrologAtom varAtom = new("var");
        PrologAtom atomAtom = new("atom");
        PrologAtom otherAtom = new("other");

        PrologDatabase db = PrologDSL.Database.Create([
            TypeClassifier.Classify.Rule()
                .AddDefinition(rule => rule
                    .Variables("X", (x, r) => r
                        .Arguments(x, varAtom)
                        .Body(PrologDSL.Goals.Var(x).And(PrologDSL.Goals.Cut))))
                .AddDefinition(rule => rule
                    .Variables("X", (x, r) => r
                        .Arguments(x, atomAtom)
                        .Body(PrologDSL.Goals.Atom(x).And(PrologDSL.Goals.Cut)))),

            TypeClassifier.Classify.Fact(PrologDSL.Wildcard, otherAtom),

            TypeClassifier.NotIdentical.Rule()
                .AddDefinition(rule => rule
                    .Variables("X", "Y", (x, y, r) => r
                        .Arguments(x, y)
                        .Body(PrologDSL.Goals.NotIdentical(x, y)))),

            TypeClassifier.ComesBefore.Rule()
                .AddDefinition(rule => rule
                    .Variables("X", "Y", (x, y, r) => r
                        .Arguments(x, y)
                        .Body(PrologDSL.Goals.TermLt(x, y)))),

            TypeClassifier.UniqueSolution.Rule()
                .AddDefinition(rule => rule
                    .Variables("Goal", (goal, r) => r
                        .Arguments(goal)
                        .Body(PrologDSL.Goals.Once(new Call("call", [goal]))))),
        ]);

        string actual = PrologSerializer.Serialize(db);

        string expected = """
            classify(X, var) :-
                var(X),
                !.
            classify(X, atom) :-
                atom(X),
                !.
            classify(_, other).
            not_identical(X, Y) :-
                X \== Y.
            comes_before(X, Y) :-
                X @< Y.
            unique_solution(Goal) :-
                once(call(Goal)).

            """;

        Assert.Equal(expected, actual);
    }
}

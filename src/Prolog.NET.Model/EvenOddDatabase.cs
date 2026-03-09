using Prolog.NET.Model;
using Prolog.NET.Model.Generated;

namespace Prolog.NET.Model;

public static class EvenOddExample
{
    [GenerateRelationTypes]
    public static readonly PrologDatabase EvenOdd = new([
        PrologFact.Of("even", new PrologInteger(0)),
        PrologRule.Of("even",
            [PrologCompoundTerm.Of("s", new PrologVariable(new VariableName("N")))],
            [PrologCompoundTerm.Of("odd",  new PrologVariable(new VariableName("N")))]),
        PrologRule.Of("odd",
            [PrologCompoundTerm.Of("s", new PrologVariable(new VariableName("N")))],
            [PrologCompoundTerm.Of("even", new PrologVariable(new VariableName("N")))]),
    ]);

    // Verification: Even.QueryZero with an AtomArgument compiles (plan step 4)
    public static Even.Query<Even.Zero> ExampleZeroQuery()
        => Even.QueryZero(
            new QueryArgument<Even.Zero>.AtomArgument(
                new Atom<Even.Zero>(new Even.Zero())));

    // Verification: Even.S<TOdd> correctly constrains TOdd : IOdd (plan step 5)
    // Odd.S<Even.Zero> satisfies TOdd : Odd.IOdd ✓
    public static Even.Query<Even.S<Odd.S<Even.Zero>>> ExampleNestedQuery()
        => Even.QueryS(
            new QueryArgument<Even.S<Odd.S<Even.Zero>>>.AtomArgument(
                new Atom<Even.S<Odd.S<Even.Zero>>>(
                    new Even.S<Odd.S<Even.Zero>>(new Odd.S<Even.Zero>(new Even.Zero())))));
}

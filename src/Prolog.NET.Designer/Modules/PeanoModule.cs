using Prolog.NET.Model;

namespace Prolog.NET.Designer.Modules;

public static partial class PeanoModule
{
    [PrologModule("peano")]
    [PrologFunctor<PrologArgument>("s")]
    public partial record S : IFunctor;

    [PrologModule("peano")]
    [PrologRelationName("even")]
    [PrologRelation<PrologArgument>]
    [PrologRelation<S>]
    public partial record Even : IRelation;

    [PrologModule("peano")]
    [PrologRelationName("odd")]
    [PrologRelation<PrologArgument>]
    [PrologRelation<S>]
    public partial record Odd : IRelation;
}

using Prolog.NET.Model;

namespace Prolog.NET.Designer.Modules;

public static partial class FamilyModule
{
    [PrologModule("family")]
    [PrologRelationName("male")]
    [PrologRelation<PrologArgument>]
    public partial record Male : IRelation;

    [PrologModule("family")]
    [PrologRelationName("female")]
    [PrologRelation<PrologArgument>]
    public partial record Female : IRelation;

    [PrologModule("family")]
    [PrologRelationName("parent")]
    [PrologRelation<PrologArgument, PrologArgument>]
    public partial record Parent : IRelation;

    [PrologModule("family")]
    [PrologRelationName("father")]
    [PrologRelation<PrologArgument, PrologArgument>]
    public partial record Father : IRelation;
}

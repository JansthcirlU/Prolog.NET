using Prolog.NET.Model;

namespace Prolog.NET.Designer.Modules;

public static partial class GenealogyModule
{
    [PrologModule("genealogy")]
    [PrologRelationName("grandfather")]
    [PrologRelation<PrologArgument, PrologArgument>]
    public partial record Grandfather : IRelation;
}

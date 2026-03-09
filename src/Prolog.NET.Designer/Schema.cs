using Prolog.NET.Model;

namespace Prolog.NET.Designer;

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

[PrologModule("genealogy")]
[PrologRelationName("grandfather")]
[PrologRelation<PrologArgument, PrologArgument>]
public partial record Grandfather : IRelation;

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

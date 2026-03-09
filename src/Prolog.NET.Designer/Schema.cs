using Prolog.NET.Model;

namespace Prolog.NET.Designer;

[PrologRelationName("male")]
[PrologRelation<PrologArgument>]
public partial record Male : IRelation;

[PrologRelationName("female")]
[PrologRelation<PrologArgument>]
public partial record Female : IRelation;

[PrologRelationName("parent")]
[PrologRelation<PrologArgument, PrologArgument>]
public partial record Parent : IRelation;

[PrologRelationName("father")]
[PrologRelation<PrologArgument, PrologArgument>]
public partial record Father : IRelation;

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

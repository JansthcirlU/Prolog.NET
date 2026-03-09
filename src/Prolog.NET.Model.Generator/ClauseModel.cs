// Internal data model used between the analysis and emission steps.
using System.Collections.Generic;

namespace Prolog.NET.Model.Generator;

/// <summary>A single argument position in a clause head.</summary>
internal abstract record HeadArg
{
    /// <summary>An integer literal, e.g. <c>0</c>.</summary>
    internal sealed record IntLiteral(long Value) : HeadArg;

    /// <summary>An atom literal, e.g. <c>foo</c>.</summary>
    internal sealed record AtomLiteral(string Name) : HeadArg;

    /// <summary>A compound term whose inner args are recursively modelled.</summary>
    internal sealed record Compound(string Functor, IReadOnlyList<HeadArg> Args) : HeadArg;

    /// <summary>A free variable — name is used to look up body constraints.</summary>
    internal sealed record Variable(string Name) : HeadArg;
}

/// <summary>A goal term appearing in a rule body.</summary>
internal abstract record BodyGoal
{
    /// <summary>A call to a known functor with specific variable arguments.</summary>
    internal sealed record Call(string Functor, IReadOnlyList<string> ArgVariableNames) : BodyGoal;

    /// <summary>Any goal whose structure we don't analyse further.</summary>
    internal sealed record Opaque : BodyGoal;
}

/// <summary>One analysed clause (fact or rule) for a single relation.</summary>
internal sealed record ClauseModel(
    string Functor,
    IReadOnlyList<HeadArg> HeadArgs,
    IReadOnlyList<BodyGoal> Body   // empty for facts
);

/// <summary>
/// All clauses for a single functor, ready for code generation.
/// </summary>
internal sealed record RelationModel(
    string Functor,
    IReadOnlyList<ClauseModel> Clauses
);

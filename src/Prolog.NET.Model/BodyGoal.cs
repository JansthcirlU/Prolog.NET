namespace Prolog.NET.Model;

public abstract record BodyGoal
{
    public BodyGoal And(BodyGoal other) => new Conjunction(this, other);
    public BodyGoal Or(BodyGoal other) => new Disjunction(this, other);
    public BodyGoal Not() => new Negation(this);
    public BodyGoal IfThen(BodyGoal then) => new IfThen(this, then);
}

public sealed record Call(string Functor, IReadOnlyList<PrologTerm> Args, string? Module = null) : BodyGoal;
public sealed record Conjunction(BodyGoal Left, BodyGoal Right) : BodyGoal;
public sealed record Disjunction(BodyGoal Left, BodyGoal Right) : BodyGoal;
public sealed record True : BodyGoal;
public sealed record Fail : BodyGoal;
public sealed record Cut : BodyGoal;
public sealed record Negation(BodyGoal Goal) : BodyGoal;
public sealed record IfThen(BodyGoal Condition, BodyGoal Then) : BodyGoal;
public sealed record Once(BodyGoal Goal) : BodyGoal;
public sealed record BinaryGoal(string Operator, PrologTerm Left, PrologTerm Right) : BodyGoal;

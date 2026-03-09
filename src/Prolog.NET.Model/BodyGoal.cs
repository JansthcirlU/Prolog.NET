namespace Prolog.NET.Model;

public abstract record BodyGoal
{
    public BodyGoal And(BodyGoal other) => new Conjunction(this, other);
    public BodyGoal Or(BodyGoal other) => new Disjunction(this, other);
}

public sealed record Call(string Functor, IReadOnlyList<PrologTerm> Args) : BodyGoal;
public sealed record Conjunction(BodyGoal Left, BodyGoal Right) : BodyGoal;
public sealed record Disjunction(BodyGoal Left, BodyGoal Right) : BodyGoal;
public sealed record True : BodyGoal;

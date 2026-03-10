using Prolog.NET.Model;

namespace Prolog.NET.Model.Tests;

public class OperatorTests
{
    private static readonly PrologVariable X = new("X");
    private static readonly PrologVariable Y = new("Y");

    private static string SerializeBody(BodyGoal body, IReadOnlyList<PrologTerm>? args = null)
    {
        PrologRuleClause rule = new("test", args ?? [X], body);
        PrologDatabase db = new([rule]);
        string serialized = PrologSerializer.Serialize(db);
        int bodyStart = serialized.IndexOf(":-\n    ", StringComparison.Ordinal) + ":-\n    ".Length;
        int bodyEnd = serialized.LastIndexOf('.');
        return serialized[bodyStart..bodyEnd];
    }

    [Fact]
    public void Fail_Serializes()
    {
        string actual = SerializeBody(new Fail());
        Assert.Equal("fail", actual);
    }

    [Fact]
    public void Cut_Serializes()
    {
        string actual = SerializeBody(new Cut());
        Assert.Equal("!", actual);
    }

    [Fact]
    public void Negation_SimpleGoal_Serializes()
    {
        string actual = SerializeBody(new Negation(new Call("foo", [X])));
        Assert.Equal("""\+(foo(X))""", actual);
    }

    [Fact]
    public void Negation_Conjunction_WrapsInParens()
    {
        Conjunction conj = new(new Call("foo", [X]), new Call("bar", [Y]));
        string actual = SerializeBody(new Negation(conj), [X, Y]);
        Assert.Equal("\\+((foo(X),\n    bar(Y)))", actual);
    }

    [Fact]
    public void IfThen_Serializes()
    {
        IfThen ifthen = new(new Call("foo", [X]), new Call("bar", [Y]));
        string actual = SerializeBody(ifthen, [X, Y]);
        Assert.Equal("(foo(X) -> bar(Y))", actual);
    }

    [Fact]
    public void IfThenElse_Serializes()
    {
        IfThen ifthen = new(new Call("foo", [X]), new Call("bar", [Y]));
        BodyGoal ifthenelse = ifthen.Or(new Call("baz", [X]));
        string actual = SerializeBody(ifthenelse, [X, Y]);
        Assert.Equal("((foo(X) -> bar(Y))\n    ; baz(X))", actual);
    }

    [Fact]
    public void Once_SimpleGoal_Serializes()
    {
        string actual = SerializeBody(new Once(new Call("foo", [X])));
        Assert.Equal("once(foo(X))", actual);
    }

    [Fact]
    public void Once_Conjunction_WrapsInParens()
    {
        Conjunction conj = new(new Call("foo", [X]), new Call("bar", [Y]));
        string actual = SerializeBody(new Once(conj), [X, Y]);
        Assert.Equal("once((foo(X),\n    bar(Y)))", actual);
    }

    [Fact]
    public void Unify_Serializes()
    {
        string actual = SerializeBody(PrologDSL.Goals.Unify(X, Y), [X, Y]);
        Assert.Equal("X = Y", actual);
    }

    [Fact]
    public void NotUnify_Serializes()
    {
        string actual = SerializeBody(PrologDSL.Goals.NotUnify(X, Y), [X, Y]);
        Assert.Equal("""X \= Y""", actual);
    }

    [Fact]
    public void Identical_Serializes()
    {
        string actual = SerializeBody(PrologDSL.Goals.Identical(X, Y), [X, Y]);
        Assert.Equal("X == Y", actual);
    }

    [Fact]
    public void NotIdentical_Serializes()
    {
        string actual = SerializeBody(PrologDSL.Goals.NotIdentical(X, Y), [X, Y]);
        Assert.Equal("""X \== Y""", actual);
    }

    [Fact]
    public void TermLt_Serializes()
    {
        string actual = SerializeBody(PrologDSL.Goals.TermLt(X, Y), [X, Y]);
        Assert.Equal("X @< Y", actual);
    }

    [Fact]
    public void Is_Serializes()
    {
        string actual = SerializeBody(PrologDSL.Goals.Is(X, Y), [X, Y]);
        Assert.Equal("X is Y", actual);
    }

    [Fact]
    public void ArithEq_Serializes()
    {
        string actual = SerializeBody(PrologDSL.Goals.ArithEq(X, Y), [X, Y]);
        Assert.Equal("X =:= Y", actual);
    }

    [Fact]
    public void ArithLeq_Serializes()
    {
        string actual = SerializeBody(PrologDSL.Goals.ArithLeq(X, Y), [X, Y]);
        Assert.Equal("X =< Y", actual);
    }

    [Fact]
    public void Var_Serializes()
    {
        string actual = SerializeBody(PrologDSL.Goals.Var(X));
        Assert.Equal("var(X)", actual);
    }

    [Fact]
    public void Atom_Serializes()
    {
        string actual = SerializeBody(PrologDSL.Goals.Atom(X));
        Assert.Equal("atom(X)", actual);
    }
}

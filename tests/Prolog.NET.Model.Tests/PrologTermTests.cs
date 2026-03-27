namespace Prolog.NET.Model.Tests;

public class PrologTermTests
{
    // --- PrologFloat ---

    [Fact]
    public void PrologFloat_serializes_positive()
    {
        PrologDatabase db = PrologDSL.Database.Create([
            new PrologFact("weight", [new PrologFloat(3.14)])
        ]);
        string result = PrologSerializer.Serialize(db);
        Assert.Contains("weight(3.14).", result);
    }

    [Fact]
    public void PrologFloat_serializes_negative()
    {
        PrologDatabase db = PrologDSL.Database.Create([
            new PrologFact("temp", [new PrologFloat(-273.15)])
        ]);
        string result = PrologSerializer.Serialize(db);
        Assert.Contains("temp(-273.15).", result);
    }

    [Fact]
    public void PrologFloat_serializes_whole_number()
    {
        PrologDatabase db = PrologDSL.Database.Create([
            new PrologFact("count", [new PrologFloat(42.0)])
        ]);
        string result = PrologSerializer.Serialize(db);
        Assert.Contains("count(42).", result);
    }

    [Fact]
    public void PrologFloat_via_DSL()
    {
        PrologFloat f = PrologDSL.Atom.CreateFloat(2.718);
        Assert.Equal(2.718, f.Value);
    }

    // --- PrologString ---

    [Fact]
    public void PrologString_serializes_simple()
    {
        PrologDatabase db = PrologDSL.Database.Create([
            new PrologFact("greeting", [new PrologString("hello world")])
        ]);
        string result = PrologSerializer.Serialize(db);
        Assert.Contains("greeting(\"hello world\").", result);
    }

    [Fact]
    public void PrologString_serializes_with_escaping()
    {
        PrologDatabase db = PrologDSL.Database.Create([
            new PrologFact("msg", [new PrologString("say \"hi\"")])
        ]);
        string result = PrologSerializer.Serialize(db);
        Assert.Contains("msg(\"say \\\"hi\\\"\").", result);
    }

    [Fact]
    public void PrologString_allows_empty()
    {
        PrologString s = new("");
        Assert.Equal("", s.Value);
    }

    [Fact]
    public void PrologString_via_DSL()
    {
        PrologString s = PrologDSL.Atom.CreateString("test");
        Assert.Equal("test", s.Value);
    }

    // --- PrologNil ---

    [Fact]
    public void PrologNil_serializes_as_empty_list()
    {
        PrologDatabase db = PrologDSL.Database.Create([
            new PrologFact("empty", [PrologList.Nil.Instance])
        ]);
        string result = PrologSerializer.Serialize(db);
        Assert.Contains("empty([]).", result);
    }

    [Fact]
    public void PrologNil_via_DSL()
    {
        PrologTerm nil = PrologDSL.Nil;
        Assert.Same(PrologList.Nil.Instance, nil);
    }

    // --- PrologList ---

    [Fact]
    public void PrologList_serializes_proper_list()
    {
        PrologAtom a = new("a");
        PrologAtom b = new("b");
        PrologAtom c = new("c");

        PrologList list = PrologDSL.List([a, b, c]);

        PrologDatabase db = PrologDSL.Database.Create([
            new PrologFact("items", [list])
        ]);
        string result = PrologSerializer.Serialize(db);
        Assert.Contains("items([a, b, c]).", result);
    }

    [Fact]
    public void PrologList_serializes_single_element()
    {
        PrologList list = PrologDSL.List([new PrologAtom("x")]);

        PrologDatabase db = PrologDSL.Database.Create([
            new PrologFact("single", [list])
        ]);
        string result = PrologSerializer.Serialize(db);
        Assert.Contains("single([x]).", result);
    }

    [Fact]
    public void PrologList_serializes_partial_list()
    {
        // [a, b | X] — a list whose tail is a variable
        PrologVariable x = new("X");
        PrologList list = new PrologList.Cons(new PrologAtom("a"), new PrologList.Cons(new PrologAtom("b"), x));

        PrologDatabase db = PrologDSL.Database.Create([
            new PrologFact("partial", [list])
        ]);
        string result = PrologSerializer.Serialize(db);
        Assert.Contains("partial([a, b | X]).", result);
    }

    [Fact]
    public void PrologList_serializes_nested_list()
    {
        // [[1, 2], [3, 4]]
        PrologList inner1 = PrologDSL.List([new PrologInteger(1), new PrologInteger(2)]);
        PrologList inner2 = PrologDSL.List([new PrologInteger(3), new PrologInteger(4)]);
        PrologList outer = PrologDSL.List([inner1, inner2]);

        PrologDatabase db = PrologDSL.Database.Create([
            new PrologFact("nested", [outer])
        ]);
        string result = PrologSerializer.Serialize(db);
        Assert.Contains("nested([[1, 2], [3, 4]]).", result);
    }

    [Fact]
    public void PrologList_serializes_mixed_types()
    {
        PrologList list = PrologDSL.List([
            new PrologAtom("hello"),
            new PrologInteger(42),
            new PrologFloat(3.14)
        ]);

        PrologDatabase db = PrologDSL.Database.Create([
            new PrologFact("mixed", [list])
        ]);
        string result = PrologSerializer.Serialize(db);
        Assert.Contains("mixed([hello, 42, 3.14]).", result);
    }

    // --- PrologInteger (renamed from PrologIntAtom) ---

    [Fact]
    public void PrologInteger_still_works_after_rename()
    {
        PrologInteger i = PrologDSL.Atom.CreateInt(99);
        Assert.Equal(99, i.Value);

        PrologDatabase db = PrologDSL.Database.Create([
            new PrologFact("num", [i])
        ]);
        string result = PrologSerializer.Serialize(db);
        Assert.Contains("num(99).", result);
    }
}

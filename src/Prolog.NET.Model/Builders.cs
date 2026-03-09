namespace Prolog.NET.Model;

public sealed class DefinitionBuilder
{
    private IReadOnlyList<PrologTerm>? _args;
    private BodyGoal? _body;

    public DefinitionBuilder Arguments(params PrologTerm[] args)
    {
        _args = args;
        return this;
    }

    public DefinitionBuilder Body(BodyGoal body)
    {
        _body = body;
        return this;
    }

    public DefinitionBuilder Body()
    {
        _body = new True();
        return this;
    }

    internal PrologRuleClause Build(string functor)
    {
        if (_args is null)
        {
            throw new InvalidOperationException("Arguments must be set before building a rule clause.");
        }

        if (_body is null)
        {
            throw new InvalidOperationException("Body must be set before building a rule clause.");
        }

        return new PrologRuleClause(functor, _args, _body);
    }

    public DefinitionBuilder Variables(
        string n1,
        Func<PrologVariable, DefinitionBuilder, DefinitionBuilder> build)
    {
        PrologVariable v1 = new(n1);
        return build(v1, this);
    }

    public DefinitionBuilder Variables(
        string n1, string n2,
        Func<PrologVariable, PrologVariable, DefinitionBuilder, DefinitionBuilder> build)
    {
        PrologVariable v1 = new(n1);
        PrologVariable v2 = new(n2);
        return build(v1, v2, this);
    }

    public DefinitionBuilder Variables(
        string n1, string n2, string n3,
        Func<PrologVariable, PrologVariable, PrologVariable, DefinitionBuilder, DefinitionBuilder> build)
    {
        PrologVariable v1 = new(n1);
        PrologVariable v2 = new(n2);
        PrologVariable v3 = new(n3);
        return build(v1, v2, v3, this);
    }

    public DefinitionBuilder Variables(
        string n1, string n2, string n3, string n4,
        Func<PrologVariable, PrologVariable, PrologVariable, PrologVariable, DefinitionBuilder, DefinitionBuilder> build)
    {
        PrologVariable v1 = new(n1);
        PrologVariable v2 = new(n2);
        PrologVariable v3 = new(n3);
        PrologVariable v4 = new(n4);
        return build(v1, v2, v3, v4, this);
    }
}

public sealed class RuleBuilder
{
    private readonly string _functor;
    private readonly List<PrologRuleClause> _definitions = new();

    public RuleBuilder(string functor)
    {
        _functor = functor;
    }

    public RuleBuilder AddDefinition(Func<DefinitionBuilder, DefinitionBuilder> build)
    {
        DefinitionBuilder db = new();
        build(db);
        _definitions.Add(db.Build(_functor));
        return this;
    }

    internal PrologDatabaseItem Build() => PrologDatabaseItem.FromEntries(_definitions);
}

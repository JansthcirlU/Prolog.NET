using System.Text;

namespace Prolog.NET.Model;

public static class PrologSerializer
{
    public static string Serialize(PrologDatabase database)
    {
        ArgumentNullException.ThrowIfNull(database);

        StringBuilder sb = new();
        foreach (PrologDatabaseEntry entry in database.Entries)
        {
            sb.AppendLine(SerializeEntry(entry));
        }
        return sb.ToString();
    }

    public static string Serialize(PrologModule module)
    {
        ArgumentNullException.ThrowIfNull(module);

        IReadOnlyList<string> exports = [..module.Database.Entries
                .Where(e => e is not PrologFact f || f.Module is null || f.Module == module.Name)
                .Select(e => e switch
                {
                    PrologFact f => $"{f.Functor}/{f.Args.Count}",
                    PrologRuleClause r => $"{r.Functor}/{r.Args.Count}",
                    PrologMultifileDeclaration d => $"{d.Functor}/{d.Arity}",
                    _ => null
                })
                .OfType<string>()
                .Distinct()];

        List<string> multifileDecls = [..module.Database.Entries
            .OfType<PrologMultifileDeclaration>()
            .Select(d => $"{d.Functor}/{d.Arity}")
            .Distinct()];

        StringBuilder sb = new();
        sb.AppendLine($":- module({module.Name}, [{string.Join(", ", exports)}]).");
        sb.AppendLine();

        List<string> imports = [..CollectImports(module)];
        if (imports.Count > 0)
        {
            foreach (string import in imports)
            {
                sb.AppendLine($":- use_module({import}).");
            }
            sb.AppendLine();
        }

        if (multifileDecls.Count > 0)
        {
            foreach (string decl in multifileDecls)
            {
                sb.AppendLine($":- multifile {decl}.");
            }
            sb.AppendLine();
        }

        foreach (PrologDatabaseEntry entry in module.Database.Entries
            .Where(e => e is not PrologMultifileDeclaration))
        {
            sb.AppendLine(SerializeEntry(entry, module.Name));
        }

        return sb.ToString();
    }

    private static List<string> CollectImports(PrologModule module)
        => [..module.Database.Entries
            .SelectMany<PrologDatabaseEntry, string>(e => e switch
            {
                PrologRuleClause rule => CollectModulesFromGoal(rule.Body, module.Name),
                PrologFact fact when fact.Module is not null && fact.Module != module.Name => [fact.Module],
                _ => []
            })
            .Distinct()
            .Order()];

    private static IEnumerable<string> CollectModulesFromGoal(BodyGoal goal, string currentModule)
        => goal switch
        {
            Call call when call.Module is not null && call.Module != currentModule => [call.Module],
            Conjunction conj => CollectModulesFromGoal(conj.Left, currentModule)
                .Concat(CollectModulesFromGoal(conj.Right, currentModule)),
            Disjunction disj => CollectModulesFromGoal(disj.Left, currentModule)
                .Concat(CollectModulesFromGoal(disj.Right, currentModule)),
            _ => []
        };

    private static string SerializeEntry(PrologDatabaseEntry entry, string? moduleName = null)
    {
        string qualifier(PrologFact f) =>
            f.Module is not null && f.Module != moduleName ? $"{f.Module}:" : "";

        return entry switch
        {
            PrologFact fact when fact.Args.Count == 0 => $"{qualifier(fact)}{SerializeAtom(fact.Functor)}.",
            PrologFact fact => $"{qualifier(fact)}{SerializeAtom(fact.Functor)}({string.Join(", ", fact.Args.Select(SerializeTerm))}).",
            PrologRuleClause rule => $"{SerializeAtom(rule.Functor)}({string.Join(", ", rule.Args.Select(SerializeTerm))}) :-\n    {SerializeBody(rule.Body, moduleName)}.",
            _ => throw new ArgumentException($"Unknown entry type: {entry.GetType().Name}"),
        };
    }

    private static string SerializeTerm(PrologTerm term) => term switch
    {
        PrologAtom atom => SerializeAtom(atom.Name),
        PrologIntAtom intAtom => intAtom.Value.ToString(),
        PrologWildcard => "_",
        PrologVariable variable => variable.Name,
        PrologCompound compound => $"{SerializeAtom(compound.Functor)}({string.Join(", ", compound.Args.Select(SerializeTerm))})",
        _ => throw new ArgumentException($"Unknown term type: {term.GetType().Name}"),
    };

    private static string SerializeBody(BodyGoal goal, string? moduleName = null) => goal switch
    {
        True => "true",
        Fail => "fail",
        Cut => "!",
        Negation neg => $"\\+({GoalAsArg(neg.Goal, moduleName)})",
        IfThen ifthen => $"({SerializeBody(ifthen.Condition, moduleName)} -> {SerializeBody(ifthen.Then, moduleName)})",
        Once once => $"once({GoalAsArg(once.Goal, moduleName)})",
        BinaryGoal bin => $"{SerializeTerm(bin.Left)} {bin.Operator} {SerializeTerm(bin.Right)}",
        Call call when call.Module is not null && call.Module != moduleName
            => $"{call.Module}:{SerializeAtom(call.Functor)}({string.Join(", ", call.Args.Select(SerializeTerm))})",
        Call call => $"{SerializeAtom(call.Functor)}({string.Join(", ", call.Args.Select(SerializeTerm))})",
        Conjunction conj => string.Join(",\n    ", FlattenConjunction(conj).Select(g => SerializeBody(g, moduleName))),
        Disjunction disj => $"({SerializeBody(disj.Left, moduleName)}\n    ; {SerializeBody(disj.Right, moduleName)})",
        _ => throw new ArgumentException($"Unknown body goal type: {goal.GetType().Name}"),
    };

    private static string GoalAsArg(BodyGoal goal, string? moduleName) =>
        goal is Conjunction or Disjunction or IfThen
            ? $"({SerializeBody(goal, moduleName)})"
            : SerializeBody(goal, moduleName);

    private static IEnumerable<BodyGoal> FlattenConjunction(BodyGoal goal)
    {
        if (goal is Conjunction conj)
        {
            foreach (BodyGoal left in FlattenConjunction(conj.Left))
            {
                yield return left;
            }

            foreach (BodyGoal right in FlattenConjunction(conj.Right))
            {
                yield return right;
            }
        }
        else
        {
            yield return goal;
        }
    }

    private static string SerializeAtom(string name)
    {
        if (IsUnquotedAtom(name))
        {
            return name;
        }

        return "'" + name.Replace("\\", "\\\\").Replace("'", "\\'") + "'";
    }

    private static bool IsUnquotedAtom(string name)
    {
        if (name.Length == 0)
        {
            return false;
        }

        // Special atoms that never need quoting.
        if (name is "[]" or "{}" or "!")
        {
            return true;
        }

        // Identifier atom: starts with lowercase, rest are letters/digits/underscores.
        if (char.IsLower(name[0]) && name.All(c => char.IsLetterOrDigit(c) || c == '_'))
        {
            return true;
        }

        // Symbolic atom: consists entirely of graphic characters.
        const string graphicChars = "#&*+-./:<=>?@\\^~";
        if (name.All(c => graphicChars.Contains(c)))
        {
            return true;
        }

        return false;
    }
}

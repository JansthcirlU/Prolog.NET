using System.Text;

namespace Prolog.NET.Model;

public static class PrologSerializer
{
    public static string Serialize(PrologDatabase database)
    {
        ArgumentNullException.ThrowIfNull(database);

        var sb = new StringBuilder();
        foreach (var entry in database.Entries)
        {
            sb.AppendLine(SerializeEntry(entry));
        }
        return sb.ToString();
    }

    private static string SerializeEntry(PrologDatabaseEntry entry) => entry switch
    {
        PrologFact fact when fact.Args.Count == 0 => $"{SerializeAtom(fact.Functor)}.",
        PrologFact fact => $"{SerializeAtom(fact.Functor)}({string.Join(", ", fact.Args.Select(SerializeTerm))}).",
        PrologRuleClause rule => $"{SerializeAtom(rule.Functor)}({string.Join(", ", rule.Args.Select(SerializeTerm))}) :-\n    {SerializeBody(rule.Body)}.",
        _ => throw new ArgumentException($"Unknown entry type: {entry.GetType().Name}"),
    };

    private static string SerializeTerm(PrologTerm term) => term switch
    {
        PrologAtom atom => SerializeAtom(atom.Name),
        PrologIntAtom intAtom => intAtom.Value.ToString(),
        PrologVariable variable => variable.Name,
        PrologCompound compound => $"{SerializeAtom(compound.Functor)}({string.Join(", ", compound.Args.Select(SerializeTerm))})",
        _ => throw new ArgumentException($"Unknown term type: {term.GetType().Name}"),
    };

    private static string SerializeBody(BodyGoal goal) => goal switch
    {
        True => "true",
        Call call => $"{SerializeAtom(call.Functor)}({string.Join(", ", call.Args.Select(SerializeTerm))})",
        Conjunction conj => string.Join(",\n    ", FlattenConjunction(conj).Select(SerializeBody)),
        Disjunction disj => $"({SerializeBody(disj.Left)}\n    ; {SerializeBody(disj.Right)})",
        _ => throw new ArgumentException($"Unknown body goal type: {goal.GetType().Name}"),
    };

    private static IEnumerable<BodyGoal> FlattenConjunction(BodyGoal goal)
    {
        if (goal is Conjunction conj)
        {
            foreach (var left in FlattenConjunction(conj.Left))
                yield return left;
            foreach (var right in FlattenConjunction(conj.Right))
                yield return right;
        }
        else
        {
            yield return goal;
        }
    }

    private static string SerializeAtom(string name)
    {
        if (IsUnquotedAtom(name)) return name;
        return "'" + name.Replace("\\", "\\\\").Replace("'", "\\'") + "'";
    }

    private static bool IsUnquotedAtom(string name)
    {
        if (name.Length == 0) return false;

        // Special atoms that never need quoting.
        if (name is "[]" or "{}" or "!") return true;

        // Identifier atom: starts with lowercase, rest are letters/digits/underscores.
        if (char.IsLower(name[0]) && name.All(c => char.IsLetterOrDigit(c) || c == '_'))
            return true;

        // Symbolic atom: consists entirely of graphic characters.
        const string graphicChars = "#&*+-./:<=>?@\\^~";
        if (name.All(c => graphicChars.Contains(c)))
            return true;

        return false;
    }
}

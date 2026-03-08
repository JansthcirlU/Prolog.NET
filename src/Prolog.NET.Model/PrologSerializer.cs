using System.Text;

namespace Prolog.NET.Model;

/// <summary>
/// Serializes a <see cref="PrologDatabase"/> to a valid <c>.pl</c> file string.
/// </summary>
public static class PrologSerializer
{
    /// <summary>Serializes the database to a Prolog source string.</summary>
    public static string Serialize(PrologDatabase database)
    {
        ArgumentNullException.ThrowIfNull(database);

        var sb = new StringBuilder();
        foreach (PrologClause clause in database.Clauses)
        {
            sb.AppendLine(SerializeClause(clause));
        }
        return sb.ToString();
    }

    private static string SerializeClause(PrologClause clause) => clause switch
    {
        PrologFact fact         => $"{SerializeTerm(fact.Head)}.",
        PrologDirective dir     => $":- {SerializeTerm(dir.Goal)}.",
        PrologRule rule         => SerializeRule(rule),
        _                       => throw new ArgumentException($"Unknown clause type: {clause.GetType().Name}"),
    };

    private static string SerializeRule(PrologRule rule)
    {
        string head = SerializeTerm(rule.Head);
        string body = string.Join(",\n    ", rule.Body.Select(SerializeTerm));
        return $"{head} :-\n    {body}.";
    }

    private static string SerializeTerm(PrologTerm term) => term switch
    {
        PrologAtom atom                 => SerializeAtom(atom.Name.Value),
        PrologVariable variable         => variable.Name.Value,
        PrologInteger integer           => integer.Value.ToString(),
        PrologFloat f                   => SerializeFloat(f.Value),
        PrologCompoundTerm compound     => SerializeCompound(compound),
        PrologList list                 => SerializeList(list),
        PredicateIndicator indicator    => $"{SerializeAtom(indicator.Functor.Value)}/{indicator.Arity}",
        _                               => throw new ArgumentException($"Unknown term type: {term.GetType().Name}"),
    };

    private static string SerializeCompound(PrologCompoundTerm term)
    {
        string functor = SerializeAtom(term.Functor.Value);
        string args = string.Join(", ", term.Arguments.Select(SerializeTerm));
        return $"{functor}({args})";
    }

    private static string SerializeList(PrologList list)
    {
        if (list.Elements.Count == 0) return "[]";
        string elements = string.Join(", ", list.Elements.Select(SerializeTerm));
        return $"[{elements}]";
    }

    private static string SerializeFloat(double value)
    {
        string s = value.ToString("G");
        // Ensure there is always a decimal point so Prolog reads it as a float.
        return s.Contains('.') || s.Contains('E') ? s : s + ".0";
    }

    /// <summary>
    /// Returns the atom serialized without quotes if it is a valid unquoted Prolog atom,
    /// otherwise wraps it in single quotes with necessary escaping.
    /// </summary>
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

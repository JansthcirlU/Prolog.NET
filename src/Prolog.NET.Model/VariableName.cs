using System.Text.RegularExpressions;

namespace Prolog.NET.Model;

/// <summary>
/// A validated Prolog variable name. Must start with an uppercase letter or underscore,
/// followed by zero or more letters, digits, or underscores (e.g. <c>X</c>, <c>_G123</c>, <c>_</c>).
/// </summary>
public sealed record VariableName
{
    private static readonly Regex ValidPattern =
        new(@"^[A-Z_][a-zA-Z0-9_]*$", RegexOptions.Compiled);

    public string Value { get; }

    public VariableName(string value)
    {
        ArgumentException.ThrowIfNullOrEmpty(value);
        if (!ValidPattern.IsMatch(value))
            throw new ArgumentException(
                $"'{value}' is not a valid Prolog variable name. " +
                "Variable names must start with an uppercase letter or underscore.",
                nameof(value));
        Value = value;
    }

    public override string ToString() => Value;
}

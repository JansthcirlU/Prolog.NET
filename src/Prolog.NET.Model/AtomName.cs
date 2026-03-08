namespace Prolog.NET.Model;

/// <summary>
/// A validated Prolog atom name. Any non-empty string is accepted; the serializer
/// automatically quotes names that require it (e.g. those starting with uppercase
/// or containing special characters).
/// </summary>
public sealed record AtomName
{
    public string Value { get; }

    public AtomName(string value)
    {
        ArgumentException.ThrowIfNullOrEmpty(value);
        Value = value;
    }

    public override string ToString() => Value;
}

namespace Prolog.NET.Model;

public sealed class PrologDatabaseItem
{
    private PrologDatabaseItem(IReadOnlyList<PrologDatabaseEntry> entries)
    {
        Entries = entries;
    }

    public IReadOnlyList<PrologDatabaseEntry> Entries { get; }

    public static implicit operator PrologDatabaseItem(PrologFact fact) => new([fact]);

    public static implicit operator PrologDatabaseItem(RuleBuilder builder) => builder.Build();

    internal static PrologDatabaseItem FromEntries(IReadOnlyList<PrologDatabaseEntry> entries) => new(entries);
}

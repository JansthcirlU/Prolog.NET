namespace Prolog.NET.Model;

public sealed class PrologDatabase
{
    public PrologDatabase(IReadOnlyList<PrologDatabaseEntry> entries)
    {
        Entries = entries;
    }

    public IReadOnlyList<PrologDatabaseEntry> Entries { get; }
}

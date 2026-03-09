namespace Prolog.NET.Model;

public static class Prolog
{
    public static class Atom
    {
        public static PrologAtom Create(string name) => new(name);
        public static PrologIntAtom CreateInt(long value) => new(value);
    }

    public static class Database
    {
        public static PrologDatabase Create(IReadOnlyList<PrologDatabaseItem> items)
            => new(items.SelectMany(i => i.Entries).ToList());
    }
}

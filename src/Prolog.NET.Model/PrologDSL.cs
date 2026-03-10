namespace Prolog.NET.Model;

public static class PrologDSL
{
    public static PrologWildcard Wildcard => PrologWildcard.Instance;

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

    public static class Module
    {
        public static PrologModule Create(string name, IReadOnlyList<PrologDatabaseItem> items)
            => new(name, new PrologDatabase([.. items.SelectMany(i => i.Entries)]));
    }

    public static class Goals
    {
        // Control flow
        public static BodyGoal Cut => new Cut();
        public static BodyGoal Fail => new Fail();
        public static BodyGoal Not(BodyGoal goal) => new Negation(goal);
        public static BodyGoal IfThen(BodyGoal cond, BodyGoal then) => new IfThen(cond, then);
        public static BodyGoal Once(BodyGoal goal) => new Once(goal);

        // Unification & term identity
        public static BodyGoal Unify(PrologTerm l, PrologTerm r) => new BinaryGoal("=", l, r);
        public static BodyGoal NotUnify(PrologTerm l, PrologTerm r) => new BinaryGoal("\\=", l, r);
        public static BodyGoal Identical(PrologTerm l, PrologTerm r) => new BinaryGoal("==", l, r);
        public static BodyGoal NotIdentical(PrologTerm l, PrologTerm r) => new BinaryGoal("\\==", l, r);

        // Term ordering
        public static BodyGoal TermLt(PrologTerm l, PrologTerm r) => new BinaryGoal("@<", l, r);
        public static BodyGoal TermGt(PrologTerm l, PrologTerm r) => new BinaryGoal("@>", l, r);
        public static BodyGoal TermLeq(PrologTerm l, PrologTerm r) => new BinaryGoal("@=<", l, r);
        public static BodyGoal TermGeq(PrologTerm l, PrologTerm r) => new BinaryGoal("@>=", l, r);

        // Arithmetic
        public static BodyGoal Is(PrologTerm l, PrologTerm r) => new BinaryGoal("is", l, r);
        public static BodyGoal ArithEq(PrologTerm l, PrologTerm r) => new BinaryGoal("=:=", l, r);
        public static BodyGoal ArithNeq(PrologTerm l, PrologTerm r) => new BinaryGoal("=\\=", l, r);
        public static BodyGoal ArithLt(PrologTerm l, PrologTerm r) => new BinaryGoal("<", l, r);
        public static BodyGoal ArithGt(PrologTerm l, PrologTerm r) => new BinaryGoal(">", l, r);
        public static BodyGoal ArithLeq(PrologTerm l, PrologTerm r) => new BinaryGoal("=<", l, r);
        public static BodyGoal ArithGeq(PrologTerm l, PrologTerm r) => new BinaryGoal(">=", l, r);

        // Type checks
        public static BodyGoal Var(PrologTerm t) => new Call("var", [t]);
        public static BodyGoal NonVar(PrologTerm t) => new Call("nonvar", [t]);
        public static BodyGoal Atom(PrologTerm t) => new Call("atom", [t]);
        public static BodyGoal Number(PrologTerm t) => new Call("number", [t]);
        public static BodyGoal Integer(PrologTerm t) => new Call("integer", [t]);
        public static BodyGoal Float(PrologTerm t) => new Call("float", [t]);
        public static BodyGoal Compound(PrologTerm t) => new Call("compound", [t]);
        public static BodyGoal Callable(PrologTerm t) => new Call("callable", [t]);
        public static BodyGoal IsList(PrologTerm t) => new Call("is_list", [t]);
        public static BodyGoal Atomic(PrologTerm t) => new Call("atomic", [t]);
    }
}

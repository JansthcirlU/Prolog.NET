using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Prolog.NET.Swipl.C.stddef;

[assembly: InternalsVisibleTo("Prolog.NET.Swipl.Tests")]
namespace Prolog.NET.Swipl.C;

// Initialisation
internal static unsafe partial class SwiProlog
{

    [LibraryImport("swipl", EntryPoint = "PL_initialise")]
    [return: MarshalAs(UnmanagedType.U1)]
    internal static partial bool PL_initialise(int argc, byte** argv);

    [LibraryImport("swipl", EntryPoint = "PL_is_initialised")]
    [return: MarshalAs(UnmanagedType.U1)]
    internal static partial bool PL_is_initialised(int* argc, byte*** argv);

    [LibraryImport("swipl", EntryPoint = "PL_cleanup")]
    internal static partial PL_CLEANUP_RESULT PL_cleanup(PL_CLEANUP_STATUS_AND_FLAGS statusAndFlags);

    [LibraryImport("swipl", EntryPoint = "PL_halt")]
    [return: MarshalAs(UnmanagedType.U1)]
    internal static partial bool PL_halt(PL_HALT_STATUS status);
}

// Engine management
internal static unsafe partial class SwiProlog
{
    [LibraryImport("swipl", EntryPoint = "PL_current_engine")]
    internal static partial PL_engine_t PL_current_engine();

    [LibraryImport("swipl", EntryPoint = "PL_create_engine")]
    internal static partial PL_engine_t PL_create_engine(nint attributes);

    [LibraryImport("swipl", EntryPoint = "PL_create_engine")]
    internal static partial PL_engine_t PL_create_engine(PL_thread_attr_t* attributes);

    [LibraryImport("swipl", EntryPoint = "PL_set_engine")]
    internal static partial PL_ENGINE_RESULT PL_set_engine(PL_engine_t engine, out PL_engine_t old);

    [LibraryImport("swipl", EntryPoint = "PL_set_engine")]
    internal static partial PL_ENGINE_RESULT PL_set_engine(PL_engine_t engine, nint old);

    [LibraryImport("swipl", EntryPoint = "PL_destroy_engine")]
    [return: MarshalAs(UnmanagedType.U1)]
    internal static partial bool PL_destroy_engine(PL_engine_t engine);
}

// Threads
internal static partial class SwiProlog
{
    [LibraryImport("swipl", EntryPoint = "PL_thread_self")]
    internal static partial int PL_thread_self();
}

// Terms
internal static unsafe partial class SwiProlog
{
    [LibraryImport("swipl", EntryPoint = "PL_new_term_refs")]
    internal static partial term_t PL_new_term_refs(size_t n);

    [LibraryImport("swipl", EntryPoint = "PL_put_atom_chars")]
    [return: MarshalAs(UnmanagedType.U1)]
    internal static partial bool PL_put_atom_chars(term_t t, byte* chars);
}

// Predicates
internal static unsafe partial class SwiProlog
{
    [LibraryImport("swipl", EntryPoint = "PL_predicate")]
    internal static partial predicate_t PL_predicate(byte* name, int arity, byte* module);
}

// Queries
internal static partial class SwiProlog
{
    [LibraryImport("swipl", EntryPoint = "PL_open_query")]
    internal static partial qid_t PL_open_query(module_t m, PL_Q_FLAGS flags, predicate_t pred, term_t t0);

    [LibraryImport("swipl", EntryPoint = "PL_next_solution")]
    internal static partial PL_Q_EXT_STATUS PL_next_solution(qid_t qid);

    [LibraryImport("swipl", EntryPoint = "PL_close_query")]
    internal static partial PL_Q_EXT_STATUS PL_close_query(qid_t qid);

    [LibraryImport("swipl", EntryPoint = "PL_exception")]
    internal static partial term_t PL_exception(qid_t qid);
}

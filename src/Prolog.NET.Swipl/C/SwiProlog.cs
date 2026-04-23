using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: InternalsVisibleTo("Prolog.NET.Swipl.Tests")]
namespace Prolog.NET.Swipl.C;

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

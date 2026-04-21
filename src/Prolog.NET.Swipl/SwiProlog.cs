using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: InternalsVisibleTo("Prolog.NET.Swipl.Tests")]
namespace Prolog.NET.Swipl;

internal static unsafe partial class SwiProlog
{
    [LibraryImport("swipl", EntryPoint = "PL_initialise")]
    [return: MarshalAs(UnmanagedType.U1)]
    internal static partial bool PL_initialise(int argc, char **argv);

    [LibraryImport("swipl", EntryPoint = "PL_is_initialised")]
    [return: MarshalAs(UnmanagedType.U1)]
    internal static partial bool PL_is_initialised(int *argc, char ***argv);

    [LibraryImport("swipl", EntryPoint = "PL_halt")]
    [return: MarshalAs(UnmanagedType.U1)]
    internal static partial bool PL_halt(int status);
}

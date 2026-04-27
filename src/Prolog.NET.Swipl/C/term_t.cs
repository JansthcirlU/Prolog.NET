using System.Runtime.InteropServices;
using Prolog.NET.Swipl.C.stdint;

namespace Prolog.NET.Swipl.C;

[StructLayout(LayoutKind.Sequential)]
internal readonly struct term_t
{
    public readonly uintptr_t handle;
}

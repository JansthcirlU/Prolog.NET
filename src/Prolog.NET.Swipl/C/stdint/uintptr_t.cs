using System.Runtime.InteropServices;

namespace Prolog.NET.Swipl.C.stdint;

[StructLayout(LayoutKind.Sequential)]
internal readonly struct uintptr_t
{
    public readonly nuint handle;
}

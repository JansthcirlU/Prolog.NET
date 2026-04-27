using System.Runtime.InteropServices;

namespace Prolog.NET.Swipl.C.stddef;

[StructLayout(LayoutKind.Sequential)]
internal readonly struct size_t
{
    public readonly nuint handle;
}

using System.Runtime.InteropServices;

namespace Prolog.NET.Swipl.C;

[StructLayout(LayoutKind.Sequential)]
internal readonly struct predicate_t
{
    public readonly nint handle;
}

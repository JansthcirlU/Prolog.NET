using System.Runtime.InteropServices;

namespace Prolog.NET.Swipl.C;

[StructLayout(LayoutKind.Sequential)]
internal readonly struct module_t
{
    public readonly nint handle;
}

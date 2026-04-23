using System.Runtime.InteropServices;

namespace Prolog.NET.Swipl.C;

[StructLayout(LayoutKind.Sequential)]
internal struct PL_engine_t
{
    public static readonly PL_engine_t PL_ENGINE_MAIN = new() { handle = 0x1 };
    public static readonly PL_engine_t PL_ENGINE_CURRENT = new() { handle = 0x2 };
    public static readonly PL_engine_t PL_ENGINE_NONE = new() { handle = 0x3 };
    public nint handle;
}

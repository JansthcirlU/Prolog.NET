using System.Runtime.InteropServices;

namespace Prolog.NET.Swipl.C;

[StructLayout(LayoutKind.Sequential)]
internal unsafe struct PL_thread_attr_t
{
    public nuint stack_limit;
    public nuint table_space;
    public byte* alias;
    public delegate* unmanaged[Cdecl]<int, rc_cancel> cancel;
    public nint flags;
    public nuint max_queue_size;
    public byte* thread_class;
    private nint _reserved0;
    private nint _reserved1;
}

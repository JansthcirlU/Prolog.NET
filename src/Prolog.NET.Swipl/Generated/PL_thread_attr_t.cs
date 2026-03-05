namespace Prolog.NET.Swipl.Generated;

public unsafe partial struct PL_thread_attr_t
{
    [NativeTypeName("size_t")]
    public nuint stack_limit;

    [NativeTypeName("size_t")]
    public nuint table_space;

    [NativeTypeName("char *")]
    public sbyte* alias;

    [NativeTypeName("rc_cancel (*)(int)")]
    public delegate* unmanaged[Cdecl]<int, rc_cancel> cancel;

    [NativeTypeName("intptr_t")]
    public nint flags;

    [NativeTypeName("size_t")]
    public nuint max_queue_size;

    [NativeTypeName("void *[3]")]
    public _reserved_e__FixedBuffer reserved;

    public unsafe partial struct _reserved_e__FixedBuffer
    {
        public void* e0;
        public void* e1;
        public void* e2;

        public ref void* this[int index]
        {
            get
            {
                fixed (void** pThis = &e0)
                {
                    return ref pThis[index];
                }
            }
        }
    }
}

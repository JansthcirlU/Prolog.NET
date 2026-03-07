namespace Prolog.NET.Swipl.Generated;

internal unsafe partial struct pl_context_t
{
    [NativeTypeName("PL_engine_t")]
    public __PL_PL_local_data* ld;

    [NativeTypeName("struct __PL_queryFrame *")]
    public __PL_queryFrame* qf;

    [NativeTypeName("struct __PL_localFrame *")]
    public __PL_localFrame* fr;

    [NativeTypeName("__PL_code *")]
    public nuint* pc;

    [NativeTypeName("void *[10]")]
    public _reserved_e__FixedBuffer reserved;

    internal partial struct __PL_queryFrame
    {
    }

    internal partial struct __PL_localFrame
    {
    }

    public unsafe partial struct _reserved_e__FixedBuffer
    {
        public void* e0;
        public void* e1;
        public void* e2;
        public void* e3;
        public void* e4;
        public void* e5;
        public void* e6;
        public void* e7;
        public void* e8;
        public void* e9;

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

namespace Prolog.NET.Swipl.Generated;

internal unsafe partial struct pl_sigaction
{
    [NativeTypeName("void (*)(int)")]
    public delegate* unmanaged[Cdecl]<int, void> sa_cfunction;

    [NativeTypeName("predicate_t")]
    public __PL_procedure* sa_predicate;

    public int sa_flags;

    [NativeTypeName("void *[2]")]
    public _reserved_e__FixedBuffer reserved;

    public unsafe partial struct _reserved_e__FixedBuffer
    {
        public void* e0;
        public void* e1;

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

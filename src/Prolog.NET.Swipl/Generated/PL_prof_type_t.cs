namespace Prolog.NET.Swipl.Generated;

internal unsafe partial struct PL_prof_type_t
{
    [NativeTypeName("int (*)(term_t, void *)")]
    public delegate* unmanaged[Cdecl]<nuint, void*, int> unify;

    [NativeTypeName("int (*)(term_t, void **)")]
    public delegate* unmanaged[Cdecl]<nuint, void**, int> get;

    [NativeTypeName("void (*)(int)")]
    public delegate* unmanaged[Cdecl]<int, void> activate;

    [NativeTypeName("void (*)(void *)")]
    public delegate* unmanaged[Cdecl]<void*, void> release;

    [NativeTypeName("void *[4]")]
    public _dummy_e__FixedBuffer dummy;

    [NativeTypeName("intptr_t")]
    public nint magic;

    public unsafe partial struct _dummy_e__FixedBuffer
    {
        public void* e0;
        public void* e1;
        public void* e2;
        public void* e3;

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

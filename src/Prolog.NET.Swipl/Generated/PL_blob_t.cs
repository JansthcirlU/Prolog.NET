namespace Prolog.NET.Swipl.Generated;

public unsafe partial struct PL_blob_t
{
    [NativeTypeName("uintptr_t")]
    public nuint magic;

    [NativeTypeName("uintptr_t")]
    public nuint flags;

    [NativeTypeName("const char *")]
    public sbyte* name;

    [NativeTypeName("int (*)(atom_t)")]
    public delegate* unmanaged[Cdecl]<nuint, int> release;

    [NativeTypeName("int (*)(atom_t, atom_t)")]
    public delegate* unmanaged[Cdecl]<nuint, nuint, int> compare;

    [NativeTypeName("int (*)(IOSTREAM *, atom_t, int)")]
    public delegate* unmanaged[Cdecl]<io_stream*, nuint, int, int> write;

    [NativeTypeName("void (*)(atom_t)")]
    public delegate* unmanaged[Cdecl]<nuint, void> acquire;

    [NativeTypeName("int (*)(atom_t, IOSTREAM *)")]
    public delegate* unmanaged[Cdecl]<nuint, io_stream*, int> save;

    [NativeTypeName("atom_t (*)(IOSTREAM *)")]
    public delegate* unmanaged[Cdecl]<io_stream*, nuint> load;

    [NativeTypeName("size_t")]
    public nuint padding;

    [NativeTypeName("void *[9]")]
    public _reserved_e__FixedBuffer reserved;

    public int registered;

    public int rank;

    [NativeTypeName("struct PL_blob_t *")]
    public PL_blob_t* next;

    [NativeTypeName("atom_t")]
    public nuint atom_name;

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

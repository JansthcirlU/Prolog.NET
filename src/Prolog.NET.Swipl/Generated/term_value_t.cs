using System.Runtime.InteropServices;

namespace Prolog.NET.Swipl.Generated;

[StructLayout(LayoutKind.Explicit)]
internal unsafe partial struct term_value_t
{
    [FieldOffset(0)]
    [NativeTypeName("int64_t")]
    public long i;

    [FieldOffset(0)]
    public double f;

    [FieldOffset(0)]
    [NativeTypeName("char *")]
    public sbyte* s;

    [FieldOffset(0)]
    [NativeTypeName("atom_t")]
    public nuint a;

    [FieldOffset(0)]
    [NativeTypeName("__AnonymousRecord_SWI-Prolog_L217_C3")]
    public _t_e__Struct t;

    internal partial struct _t_e__Struct
    {
        [NativeTypeName("atom_t")]
        public nuint name;

        [NativeTypeName("size_t")]
        public nuint arity;
    }
}

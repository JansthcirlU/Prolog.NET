using System.Runtime.InteropServices;

namespace Prolog.NET.Swipl.Generated;

internal partial struct xpceref_t
{
    public int type;

    [NativeTypeName("__AnonymousRecord_SWI-Prolog_L1373_C3")]
    public _value_e__Union value;

    [StructLayout(LayoutKind.Explicit)]
    internal partial struct _value_e__Union
    {
        [FieldOffset(0)]
        [NativeTypeName("uintptr_t")]
        public nuint i;

        [FieldOffset(0)]
        [NativeTypeName("atom_t")]
        public nuint a;
    }
}

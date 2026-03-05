using System;

namespace Prolog.NET.Swipl.Generated;

internal unsafe partial struct PL_extension
{
    [NativeTypeName("const char *")]
    public sbyte* predicate_name;

    public short arity;

    [NativeTypeName("pl_function_t")]
    public IntPtr function;

    public short flags;
}

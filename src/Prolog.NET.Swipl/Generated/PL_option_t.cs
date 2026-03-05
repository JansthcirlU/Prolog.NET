namespace Prolog.NET.Swipl.Generated;

public unsafe partial struct PL_option_t
{
    [NativeTypeName("atom_t")]
    public nuint name;

    public _PL_opt_enum_t type;

    [NativeTypeName("const char *")]
    public sbyte* @string;
}

using System;
using System.Runtime.InteropServices;

namespace Prolog.NET.Swipl.Generated;

internal static unsafe partial class SwiPrologNative
{
    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    [return: NativeTypeName("foreign_t")]
    public static partial nuint _PL_retry([NativeTypeName("intptr_t")] nint param0);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    [return: NativeTypeName("foreign_t")]
    public static partial nuint _PL_retry_address(void* param0);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    [return: NativeTypeName("foreign_t")]
    public static partial nuint _PL_yield_address(void* param0);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_foreign_control([NativeTypeName("control_t")] __PL_foreign_context* param0);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    [return: NativeTypeName("intptr_t")]
    public static partial nint PL_foreign_context([NativeTypeName("control_t")] __PL_foreign_context* param0);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial void* PL_foreign_context_address([NativeTypeName("control_t")] __PL_foreign_context* param0);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    [return: NativeTypeName("predicate_t")]
    public static partial __PL_procedure* PL_foreign_context_predicate([NativeTypeName("control_t")] __PL_foreign_context* param0);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial void PL_register_extensions([NativeTypeName("const PL_extension *")] PL_extension* e);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial void PL_register_extensions_in_module([NativeTypeName("const char *")] sbyte* module, [NativeTypeName("const PL_extension *")] PL_extension* e);

    [DllImport("swipl", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern int PL_register_foreign([NativeTypeName("const char *")] sbyte* name, int arity, [NativeTypeName("pl_function_t")] IntPtr func, int flags, __arglist);

    [DllImport("swipl", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern int PL_register_foreign_in_module([NativeTypeName("const char *")] sbyte* module, [NativeTypeName("const char *")] sbyte* name, int arity, [NativeTypeName("pl_function_t")] IntPtr func, int flags, __arglist);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial void PL_load_extensions([NativeTypeName("const PL_extension *")] PL_extension* e);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial void PL_license([NativeTypeName("const char *")] sbyte* license, [NativeTypeName("const char *")] sbyte* module);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    [return: NativeTypeName("module_t")]
    public static partial __PL_module* PL_context();

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    [return: NativeTypeName("atom_t")]
    public static partial nuint PL_module_name([NativeTypeName("module_t")] __PL_module* module);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    [return: NativeTypeName("module_t")]
    public static partial __PL_module* PL_new_module([NativeTypeName("atom_t")] nuint name);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_strip_module([NativeTypeName("term_t")] nuint @in, [NativeTypeName("module_t *")] __PL_module** m, [NativeTypeName("term_t")] nuint @out);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    [return: NativeTypeName("const atom_t *")]
    public static partial nuint* _PL_atoms();

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    [return: NativeTypeName("PL_fid_t")]
    public static partial nuint PL_open_foreign_frame();

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial void PL_rewind_foreign_frame([NativeTypeName("PL_fid_t")] nuint cid);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial void PL_close_foreign_frame([NativeTypeName("PL_fid_t")] nuint cid);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial void PL_discard_foreign_frame([NativeTypeName("PL_fid_t")] nuint cid);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    [return: NativeTypeName("predicate_t")]
    public static partial __PL_procedure* PL_pred([NativeTypeName("functor_t")] nuint f, [NativeTypeName("module_t")] __PL_module* m);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    [return: NativeTypeName("predicate_t")]
    public static partial __PL_procedure* PL_predicate([NativeTypeName("const char *")] sbyte* name, int arity, [NativeTypeName("const char *")] sbyte* module);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_predicate_info([NativeTypeName("predicate_t")] __PL_procedure* pred, [NativeTypeName("atom_t *")] nuint* name, [NativeTypeName("size_t *")] nuint* arity, [NativeTypeName("module_t *")] __PL_module** module);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    [return: NativeTypeName("qid_t")]
    public static partial __PL_queryRef* PL_open_query([NativeTypeName("module_t")] __PL_module* m, int flags, [NativeTypeName("predicate_t")] __PL_procedure* pred, [NativeTypeName("term_t")] nuint t0);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_next_solution([NativeTypeName("qid_t")] __PL_queryRef* qid);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_close_query([NativeTypeName("qid_t")] __PL_queryRef* qid);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_cut_query([NativeTypeName("qid_t")] __PL_queryRef* qid);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    [return: NativeTypeName("qid_t")]
    public static partial __PL_queryRef* PL_current_query();

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    [return: NativeTypeName("PL_engine_t")]
    public static partial __PL_PL_local_data* PL_query_engine([NativeTypeName("qid_t")] __PL_queryRef* qid);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_can_yield();

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_call([NativeTypeName("term_t")] nuint t, [NativeTypeName("module_t")] __PL_module* m);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_call_predicate([NativeTypeName("module_t")] __PL_module* m, int debug, [NativeTypeName("predicate_t")] __PL_procedure* pred, [NativeTypeName("term_t")] nuint t0);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    [return: NativeTypeName("term_t")]
    public static partial nuint PL_exception([NativeTypeName("qid_t")] __PL_queryRef* qid);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_raise_exception([NativeTypeName("term_t")] nuint exception);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_throw([NativeTypeName("term_t")] nuint exception);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial void PL_clear_exception();

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    [return: NativeTypeName("term_t")]
    public static partial nuint PL_yielded([NativeTypeName("qid_t")] __PL_queryRef* qid);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_assert([NativeTypeName("term_t")] nuint term, [NativeTypeName("module_t")] __PL_module* m, int flags);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    [return: NativeTypeName("term_t")]
    public static partial nuint PL_new_term_refs(int n);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    [return: NativeTypeName("term_t")]
    public static partial nuint PL_new_term_ref();

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    [return: NativeTypeName("term_t")]
    public static partial nuint PL_copy_term_ref([NativeTypeName("term_t")] nuint from);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial void PL_reset_term_refs([NativeTypeName("term_t")] nuint r);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    [return: NativeTypeName("atom_t")]
    public static partial nuint PL_new_atom([NativeTypeName("const char *")] sbyte* s);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    [return: NativeTypeName("atom_t")]
    public static partial nuint PL_new_atom_nchars([NativeTypeName("size_t")] nuint len, [NativeTypeName("const char *")] sbyte* s);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    [return: NativeTypeName("atom_t")]
    public static partial nuint PL_new_atom_wchars([NativeTypeName("size_t")] nuint len, [NativeTypeName("const pl_wchar_t *")] ushort* s);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    [return: NativeTypeName("atom_t")]
    public static partial nuint PL_new_atom_mbchars(int rep, [NativeTypeName("size_t")] nuint len, [NativeTypeName("const char *")] sbyte* s);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    [return: NativeTypeName("const char *")]
    public static partial sbyte* PL_atom_chars([NativeTypeName("atom_t")] nuint a);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    [return: NativeTypeName("const char *")]
    public static partial sbyte* PL_atom_nchars([NativeTypeName("atom_t")] nuint a, [NativeTypeName("size_t *")] nuint* len);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_atom_mbchars([NativeTypeName("atom_t")] nuint a, [NativeTypeName("size_t *")] nuint* len, [NativeTypeName("char **")] sbyte** s, [NativeTypeName("unsigned int")] uint flags);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    [return: NativeTypeName("const wchar_t *")]
    public static partial ushort* PL_atom_wchars([NativeTypeName("atom_t")] nuint a, [NativeTypeName("size_t *")] nuint* len);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial void PL_register_atom([NativeTypeName("atom_t")] nuint a);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial void PL_unregister_atom([NativeTypeName("atom_t")] nuint a);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    [return: NativeTypeName("functor_t")]
    public static partial nuint PL_new_functor_sz([NativeTypeName("atom_t")] nuint f, [NativeTypeName("size_t")] nuint a);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    [return: NativeTypeName("functor_t")]
    public static partial nuint PL_new_functor([NativeTypeName("atom_t")] nuint f, int a);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    [return: NativeTypeName("atom_t")]
    public static partial nuint PL_functor_name([NativeTypeName("functor_t")] nuint f);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_functor_arity([NativeTypeName("functor_t")] nuint f);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    [return: NativeTypeName("size_t")]
    public static partial nuint PL_functor_arity_sz([NativeTypeName("functor_t")] nuint f);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_get_atom([NativeTypeName("term_t")] nuint t, [NativeTypeName("atom_t *")] nuint* a);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_get_bool([NativeTypeName("term_t")] nuint t, int* value);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_get_atom_chars([NativeTypeName("term_t")] nuint t, [NativeTypeName("char **")] sbyte** a);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_get_string([NativeTypeName("term_t")] nuint t, [NativeTypeName("char **")] sbyte** s, [NativeTypeName("size_t *")] nuint* len);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_get_chars([NativeTypeName("term_t")] nuint t, [NativeTypeName("char **")] sbyte** s, [NativeTypeName("unsigned int")] uint flags);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_get_list_chars([NativeTypeName("term_t")] nuint l, [NativeTypeName("char **")] sbyte** s, [NativeTypeName("unsigned int")] uint flags);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_get_atom_nchars([NativeTypeName("term_t")] nuint t, [NativeTypeName("size_t *")] nuint* len, [NativeTypeName("char **")] sbyte** a);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_get_list_nchars([NativeTypeName("term_t")] nuint l, [NativeTypeName("size_t *")] nuint* len, [NativeTypeName("char **")] sbyte** s, [NativeTypeName("unsigned int")] uint flags);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_get_nchars([NativeTypeName("term_t")] nuint t, [NativeTypeName("size_t *")] nuint* len, [NativeTypeName("char **")] sbyte** s, [NativeTypeName("unsigned int")] uint flags);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_get_integer([NativeTypeName("term_t")] nuint t, int* i);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_get_long([NativeTypeName("term_t")] nuint t, [NativeTypeName("long *")] CLong* i);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_get_intptr([NativeTypeName("term_t")] nuint t, [NativeTypeName("intptr_t *")] nint* i);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_get_pointer([NativeTypeName("term_t")] nuint t, void** ptr);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_get_float([NativeTypeName("term_t")] nuint t, double* f);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_get_functor([NativeTypeName("term_t")] nuint t, [NativeTypeName("functor_t *")] nuint* f);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_get_name_arity_sz([NativeTypeName("term_t")] nuint t, [NativeTypeName("atom_t *")] nuint* name, [NativeTypeName("size_t *")] nuint* arity);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_get_compound_name_arity_sz([NativeTypeName("term_t")] nuint t, [NativeTypeName("atom_t *")] nuint* name, [NativeTypeName("size_t *")] nuint* arity);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_get_name_arity([NativeTypeName("term_t")] nuint t, [NativeTypeName("atom_t *")] nuint* name, int* arity);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_get_compound_name_arity([NativeTypeName("term_t")] nuint t, [NativeTypeName("atom_t *")] nuint* name, int* arity);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_get_module([NativeTypeName("term_t")] nuint t, [NativeTypeName("module_t *")] __PL_module** module);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_get_arg_sz([NativeTypeName("size_t")] nuint index, [NativeTypeName("term_t")] nuint t, [NativeTypeName("term_t")] nuint a);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_get_arg(int index, [NativeTypeName("term_t")] nuint t, [NativeTypeName("term_t")] nuint a);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_get_dict_key([NativeTypeName("atom_t")] nuint key, [NativeTypeName("term_t")] nuint dict, [NativeTypeName("term_t")] nuint value);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_get_list([NativeTypeName("term_t")] nuint l, [NativeTypeName("term_t")] nuint h, [NativeTypeName("term_t")] nuint t);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_get_head([NativeTypeName("term_t")] nuint l, [NativeTypeName("term_t")] nuint h);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_get_tail([NativeTypeName("term_t")] nuint l, [NativeTypeName("term_t")] nuint t);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_get_nil([NativeTypeName("term_t")] nuint l);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_get_term_value([NativeTypeName("term_t")] nuint t, term_value_t* v);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    [return: NativeTypeName("char *")]
    public static partial sbyte* PL_quote(int chr, [NativeTypeName("const char *")] sbyte* data);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_term_type([NativeTypeName("term_t")] nuint t);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_is_variable([NativeTypeName("term_t")] nuint t);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_is_ground([NativeTypeName("term_t")] nuint t);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_is_atom([NativeTypeName("term_t")] nuint t);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_is_integer([NativeTypeName("term_t")] nuint t);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_is_string([NativeTypeName("term_t")] nuint t);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_is_float([NativeTypeName("term_t")] nuint t);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_is_rational([NativeTypeName("term_t")] nuint t);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_is_compound([NativeTypeName("term_t")] nuint t);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_is_callable([NativeTypeName("term_t")] nuint t);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_is_functor([NativeTypeName("term_t")] nuint t, [NativeTypeName("functor_t")] nuint f);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_is_list([NativeTypeName("term_t")] nuint t);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_is_dict([NativeTypeName("term_t")] nuint t);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_is_pair([NativeTypeName("term_t")] nuint t);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_is_atomic([NativeTypeName("term_t")] nuint t);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_is_number([NativeTypeName("term_t")] nuint t);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_is_acyclic([NativeTypeName("term_t")] nuint t);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_put_variable([NativeTypeName("term_t")] nuint t);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_put_atom([NativeTypeName("term_t")] nuint t, [NativeTypeName("atom_t")] nuint a);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_put_bool([NativeTypeName("term_t")] nuint t, int val);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_put_atom_chars([NativeTypeName("term_t")] nuint t, [NativeTypeName("const char *")] sbyte* chars);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_put_string_chars([NativeTypeName("term_t")] nuint t, [NativeTypeName("const char *")] sbyte* chars);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_put_chars([NativeTypeName("term_t")] nuint t, int flags, [NativeTypeName("size_t")] nuint len, [NativeTypeName("const char *")] sbyte* chars);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_put_list_chars([NativeTypeName("term_t")] nuint t, [NativeTypeName("const char *")] sbyte* chars);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_put_list_codes([NativeTypeName("term_t")] nuint t, [NativeTypeName("const char *")] sbyte* chars);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_put_atom_nchars([NativeTypeName("term_t")] nuint t, [NativeTypeName("size_t")] nuint l, [NativeTypeName("const char *")] sbyte* chars);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_put_string_nchars([NativeTypeName("term_t")] nuint t, [NativeTypeName("size_t")] nuint len, [NativeTypeName("const char *")] sbyte* chars);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_put_list_nchars([NativeTypeName("term_t")] nuint t, [NativeTypeName("size_t")] nuint l, [NativeTypeName("const char *")] sbyte* chars);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_put_list_ncodes([NativeTypeName("term_t")] nuint t, [NativeTypeName("size_t")] nuint l, [NativeTypeName("const char *")] sbyte* chars);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_put_integer([NativeTypeName("term_t")] nuint t, [NativeTypeName("long")] CLong i);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_put_pointer([NativeTypeName("term_t")] nuint t, void* ptr);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_put_float([NativeTypeName("term_t")] nuint t, double f);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_put_functor([NativeTypeName("term_t")] nuint t, [NativeTypeName("functor_t")] nuint functor);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_put_list([NativeTypeName("term_t")] nuint l);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_put_nil([NativeTypeName("term_t")] nuint l);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_put_term([NativeTypeName("term_t")] nuint t1, [NativeTypeName("term_t")] nuint t2);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_put_dict([NativeTypeName("term_t")] nuint t, [NativeTypeName("atom_t")] nuint tag, [NativeTypeName("size_t")] nuint len, [NativeTypeName("const atom_t *")] nuint* keys, [NativeTypeName("term_t")] nuint values);

    [DllImport("swipl", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern int PL_cons_functor([NativeTypeName("term_t")] nuint h, [NativeTypeName("functor_t")] nuint f, __arglist);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_cons_functor_v([NativeTypeName("term_t")] nuint h, [NativeTypeName("functor_t")] nuint fd, [NativeTypeName("term_t")] nuint a0);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_cons_list([NativeTypeName("term_t")] nuint l, [NativeTypeName("term_t")] nuint h, [NativeTypeName("term_t")] nuint t);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_unify([NativeTypeName("term_t")] nuint t1, [NativeTypeName("term_t")] nuint t2);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_unify_atom([NativeTypeName("term_t")] nuint t, [NativeTypeName("atom_t")] nuint a);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_unify_atom_chars([NativeTypeName("term_t")] nuint t, [NativeTypeName("const char *")] sbyte* chars);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_unify_list_chars([NativeTypeName("term_t")] nuint t, [NativeTypeName("const char *")] sbyte* chars);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_unify_list_codes([NativeTypeName("term_t")] nuint t, [NativeTypeName("const char *")] sbyte* chars);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_unify_string_chars([NativeTypeName("term_t")] nuint t, [NativeTypeName("const char *")] sbyte* chars);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_unify_atom_nchars([NativeTypeName("term_t")] nuint t, [NativeTypeName("size_t")] nuint l, [NativeTypeName("const char *")] sbyte* s);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_unify_list_ncodes([NativeTypeName("term_t")] nuint t, [NativeTypeName("size_t")] nuint l, [NativeTypeName("const char *")] sbyte* s);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_unify_list_nchars([NativeTypeName("term_t")] nuint t, [NativeTypeName("size_t")] nuint l, [NativeTypeName("const char *")] sbyte* s);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_unify_string_nchars([NativeTypeName("term_t")] nuint t, [NativeTypeName("size_t")] nuint len, [NativeTypeName("const char *")] sbyte* chars);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_unify_bool([NativeTypeName("term_t")] nuint t, int n);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_unify_integer([NativeTypeName("term_t")] nuint t, [NativeTypeName("intptr_t")] nint n);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_unify_float([NativeTypeName("term_t")] nuint t, double f);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_unify_pointer([NativeTypeName("term_t")] nuint t, void* ptr);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_unify_functor([NativeTypeName("term_t")] nuint t, [NativeTypeName("functor_t")] nuint f);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_unify_compound([NativeTypeName("term_t")] nuint t, [NativeTypeName("functor_t")] nuint f);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_unify_list([NativeTypeName("term_t")] nuint l, [NativeTypeName("term_t")] nuint h, [NativeTypeName("term_t")] nuint t);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_unify_nil([NativeTypeName("term_t")] nuint l);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_unify_arg_sz([NativeTypeName("size_t")] nuint index, [NativeTypeName("term_t")] nuint t, [NativeTypeName("term_t")] nuint a);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_unify_arg(int index, [NativeTypeName("term_t")] nuint t, [NativeTypeName("term_t")] nuint a);

    [DllImport("swipl", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern int PL_unify_term([NativeTypeName("term_t")] nuint t, __arglist);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_unify_chars([NativeTypeName("term_t")] nuint t, int flags, [NativeTypeName("size_t")] nuint len, [NativeTypeName("const char *")] sbyte* s);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_skip_list([NativeTypeName("term_t")] nuint list, [NativeTypeName("term_t")] nuint tail, [NativeTypeName("size_t *")] nuint* len);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_unify_wchars([NativeTypeName("term_t")] nuint t, int type, [NativeTypeName("size_t")] nuint len, [NativeTypeName("const pl_wchar_t *")] ushort* s);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_unify_wchars_diff([NativeTypeName("term_t")] nuint t, [NativeTypeName("term_t")] nuint tail, int type, [NativeTypeName("size_t")] nuint len, [NativeTypeName("const pl_wchar_t *")] ushort* s);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_get_wchars([NativeTypeName("term_t")] nuint l, [NativeTypeName("size_t *")] nuint* length, [NativeTypeName("pl_wchar_t **")] ushort** s, [NativeTypeName("unsigned int")] uint flags);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    [return: NativeTypeName("size_t")]
    public static partial nuint PL_utf8_strlen([NativeTypeName("const char *")] sbyte* s, [NativeTypeName("size_t")] nuint len);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_get_int64([NativeTypeName("term_t")] nuint t, [NativeTypeName("int64_t *")] long* i);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_get_uint64([NativeTypeName("term_t")] nuint t, [NativeTypeName("uint64_t *")] ulong* i);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_unify_int64([NativeTypeName("term_t")] nuint t, [NativeTypeName("int64_t")] long value);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_unify_uint64([NativeTypeName("term_t")] nuint t, [NativeTypeName("uint64_t")] ulong value);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_put_int64([NativeTypeName("term_t")] nuint t, [NativeTypeName("int64_t")] long i);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_put_uint64([NativeTypeName("term_t")] nuint t, [NativeTypeName("uint64_t")] ulong i);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_is_attvar([NativeTypeName("term_t")] nuint t);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_get_attr([NativeTypeName("term_t")] nuint v, [NativeTypeName("term_t")] nuint a);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_get_atom_ex([NativeTypeName("term_t")] nuint t, [NativeTypeName("atom_t *")] nuint* a);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_get_integer_ex([NativeTypeName("term_t")] nuint t, int* i);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_get_long_ex([NativeTypeName("term_t")] nuint t, [NativeTypeName("long *")] CLong* i);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_get_int64_ex([NativeTypeName("term_t")] nuint t, [NativeTypeName("int64_t *")] long* i);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_get_uint64_ex([NativeTypeName("term_t")] nuint t, [NativeTypeName("uint64_t *")] ulong* i);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_get_intptr_ex([NativeTypeName("term_t")] nuint t, [NativeTypeName("intptr_t *")] nint* i);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_get_size_ex([NativeTypeName("term_t")] nuint t, [NativeTypeName("size_t *")] nuint* i);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_get_bool_ex([NativeTypeName("term_t")] nuint t, int* i);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_get_float_ex([NativeTypeName("term_t")] nuint t, double* f);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_get_char_ex([NativeTypeName("term_t")] nuint t, int* p, int eof);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_unify_bool_ex([NativeTypeName("term_t")] nuint t, int val);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_get_pointer_ex([NativeTypeName("term_t")] nuint t, void** addrp);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_unify_list_ex([NativeTypeName("term_t")] nuint l, [NativeTypeName("term_t")] nuint h, [NativeTypeName("term_t")] nuint t);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_unify_nil_ex([NativeTypeName("term_t")] nuint l);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_get_list_ex([NativeTypeName("term_t")] nuint l, [NativeTypeName("term_t")] nuint h, [NativeTypeName("term_t")] nuint t);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_get_nil_ex([NativeTypeName("term_t")] nuint l);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_instantiation_error([NativeTypeName("term_t")] nuint culprit);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_uninstantiation_error([NativeTypeName("term_t")] nuint culprit);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_representation_error([NativeTypeName("const char *")] sbyte* resource);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_type_error([NativeTypeName("const char *")] sbyte* expected, [NativeTypeName("term_t")] nuint culprit);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_domain_error([NativeTypeName("const char *")] sbyte* expected, [NativeTypeName("term_t")] nuint culprit);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_existence_error([NativeTypeName("const char *")] sbyte* type, [NativeTypeName("term_t")] nuint culprit);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_permission_error([NativeTypeName("const char *")] sbyte* operation, [NativeTypeName("const char *")] sbyte* type, [NativeTypeName("term_t")] nuint culprit);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_resource_error([NativeTypeName("const char *")] sbyte* resource);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_syntax_error([NativeTypeName("const char *")] sbyte* msg, [NativeTypeName("IOSTREAM *")] io_stream* @in);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_is_blob([NativeTypeName("term_t")] nuint t, PL_blob_t** type);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_unify_blob([NativeTypeName("term_t")] nuint t, void* blob, [NativeTypeName("size_t")] nuint len, PL_blob_t* type);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_put_blob([NativeTypeName("term_t")] nuint t, void* blob, [NativeTypeName("size_t")] nuint len, PL_blob_t* type);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_get_blob([NativeTypeName("term_t")] nuint t, void** blob, [NativeTypeName("size_t *")] nuint* len, PL_blob_t** type);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial void* PL_blob_data([NativeTypeName("atom_t")] nuint a, [NativeTypeName("size_t *")] nuint* len, [NativeTypeName("struct PL_blob_t **")] PL_blob_t** type);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial void PL_register_blob_type(PL_blob_t* type);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial PL_blob_t* PL_find_blob_type([NativeTypeName("const char *")] sbyte* name);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_unregister_blob_type(PL_blob_t* type);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_get_file_name([NativeTypeName("term_t")] nuint n, [NativeTypeName("char **")] sbyte** name, int flags);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_get_file_nameW([NativeTypeName("term_t")] nuint n, [NativeTypeName("wchar_t **")] ushort** name, int flags);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial void PL_changed_cwd();

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    [return: NativeTypeName("char *")]
    public static partial sbyte* PL_cwd([NativeTypeName("char *")] sbyte* buf, [NativeTypeName("size_t")] nuint buflen);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_cvt_i_bool([NativeTypeName("term_t")] nuint p, int* c);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_cvt_i_char([NativeTypeName("term_t")] nuint p, [NativeTypeName("char *")] sbyte* c);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_cvt_i_schar([NativeTypeName("term_t")] nuint p, [NativeTypeName("signed char *")] sbyte* c);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_cvt_i_uchar([NativeTypeName("term_t")] nuint p, [NativeTypeName("unsigned char *")] byte* c);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_cvt_i_short([NativeTypeName("term_t")] nuint p, short* s);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_cvt_i_ushort([NativeTypeName("term_t")] nuint p, [NativeTypeName("unsigned short *")] ushort* s);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_cvt_i_int([NativeTypeName("term_t")] nuint p, int* c);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_cvt_i_uint([NativeTypeName("term_t")] nuint p, [NativeTypeName("unsigned int *")] uint* c);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_cvt_i_long([NativeTypeName("term_t")] nuint p, [NativeTypeName("long *")] CLong* c);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_cvt_i_ulong([NativeTypeName("term_t")] nuint p, [NativeTypeName("unsigned long *")] CULong* c);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_cvt_i_llong([NativeTypeName("term_t")] nuint p, [NativeTypeName("long long *")] long* c);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_cvt_i_ullong([NativeTypeName("term_t")] nuint p, [NativeTypeName("unsigned long long *")] ulong* c);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_cvt_i_int32([NativeTypeName("term_t")] nuint p, [NativeTypeName("int32_t *")] int* c);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_cvt_i_uint32([NativeTypeName("term_t")] nuint p, [NativeTypeName("uint32_t *")] uint* c);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_cvt_i_int64([NativeTypeName("term_t")] nuint p, [NativeTypeName("int64_t *")] long* c);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_cvt_i_uint64([NativeTypeName("term_t")] nuint p, [NativeTypeName("uint64_t *")] ulong* c);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_cvt_i_size_t([NativeTypeName("term_t")] nuint p, [NativeTypeName("size_t *")] nuint* c);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_cvt_i_float([NativeTypeName("term_t")] nuint p, double* c);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_cvt_i_single([NativeTypeName("term_t")] nuint p, float* c);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_cvt_i_string([NativeTypeName("term_t")] nuint p, [NativeTypeName("char **")] sbyte** c);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_cvt_i_codes([NativeTypeName("term_t")] nuint p, [NativeTypeName("char **")] sbyte** c);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_cvt_i_atom([NativeTypeName("term_t")] nuint p, [NativeTypeName("atom_t *")] nuint* c);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_cvt_i_address([NativeTypeName("term_t")] nuint p, void* c);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_cvt_o_int64([NativeTypeName("int64_t")] long c, [NativeTypeName("term_t")] nuint p);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_cvt_o_float(double c, [NativeTypeName("term_t")] nuint p);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_cvt_o_single(float c, [NativeTypeName("term_t")] nuint p);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_cvt_o_string([NativeTypeName("const char *")] sbyte* c, [NativeTypeName("term_t")] nuint p);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_cvt_o_codes([NativeTypeName("const char *")] sbyte* c, [NativeTypeName("term_t")] nuint p);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_cvt_o_atom([NativeTypeName("atom_t")] nuint c, [NativeTypeName("term_t")] nuint p);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_cvt_o_address(void* address, [NativeTypeName("term_t")] nuint p);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    [return: NativeTypeName("term_t")]
    public static partial nuint PL_new_nil_ref();

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_cvt_encoding();

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_cvt_set_encoding(int enc);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial void SP_set_state(int state);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int SP_get_state();

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_compare([NativeTypeName("term_t")] nuint t1, [NativeTypeName("term_t")] nuint t2);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_same_compound([NativeTypeName("term_t")] nuint t1, [NativeTypeName("term_t")] nuint t2);

    [DllImport("swipl", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern int PL_warning([NativeTypeName("const char *")] sbyte* fmt, __arglist);

    [DllImport("swipl", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void PL_fatal_error([NativeTypeName("const char *")] sbyte* fmt, __arglist);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    [return: NativeTypeName("record_t")]
    public static partial __PL_record* PL_record([NativeTypeName("term_t")] nuint term);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_recorded([NativeTypeName("record_t")] __PL_record* record, [NativeTypeName("term_t")] nuint term);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial void PL_erase([NativeTypeName("record_t")] __PL_record* record);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    [return: NativeTypeName("record_t")]
    public static partial __PL_record* PL_duplicate_record([NativeTypeName("record_t")] __PL_record* r);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    [return: NativeTypeName("char *")]
    public static partial sbyte* PL_record_external([NativeTypeName("term_t")] nuint t, [NativeTypeName("size_t *")] nuint* size);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_recorded_external([NativeTypeName("const char *")] sbyte* rec, [NativeTypeName("term_t")] nuint term);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_erase_external([NativeTypeName("char *")] sbyte* rec);

    [DllImport("swipl", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern int PL_set_prolog_flag([NativeTypeName("const char *")] sbyte* name, int type, __arglist);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    [return: NativeTypeName("PL_atomic_t")]
    public static partial nuint _PL_get_atomic([NativeTypeName("term_t")] nuint t);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial void _PL_put_atomic([NativeTypeName("term_t")] nuint t, [NativeTypeName("PL_atomic_t")] nuint a);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int _PL_unify_atomic([NativeTypeName("term_t")] nuint t, [NativeTypeName("PL_atomic_t")] nuint a);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int _PL_get_arg_sz([NativeTypeName("size_t")] nuint index, [NativeTypeName("term_t")] nuint t, [NativeTypeName("term_t")] nuint a);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int _PL_get_arg(int index, [NativeTypeName("term_t")] nuint t, [NativeTypeName("term_t")] nuint a);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial void PL_mark_string_buffers([NativeTypeName("buf_mark_t *")] nuint* mark);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial void PL_release_string_buffers_from_mark([NativeTypeName("buf_mark_t")] nuint mark);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_unify_stream([NativeTypeName("term_t")] nuint t, [NativeTypeName("IOSTREAM *")] io_stream* s);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_get_stream_handle([NativeTypeName("term_t")] nuint t, [NativeTypeName("IOSTREAM **")] io_stream** s);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_get_stream([NativeTypeName("term_t")] nuint t, [NativeTypeName("IOSTREAM **")] io_stream** s, int flags);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_get_stream_from_blob([NativeTypeName("atom_t")] nuint a, [NativeTypeName("IOSTREAM **")] io_stream** s, int flags);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    [return: NativeTypeName("IOSTREAM *")]
    public static partial io_stream* PL_acquire_stream([NativeTypeName("IOSTREAM *")] io_stream* s);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_release_stream([NativeTypeName("IOSTREAM *")] io_stream* s);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_release_stream_noerror([NativeTypeName("IOSTREAM *")] io_stream* s);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    [return: NativeTypeName("IOSTREAM *")]
    public static partial io_stream* PL_open_resource([NativeTypeName("module_t")] __PL_module* m, [NativeTypeName("const char *")] sbyte* name, [NativeTypeName("const char *")] sbyte* rc_class, [NativeTypeName("const char *")] sbyte* mode);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    [return: NativeTypeName("IOSTREAM **")]
    public static partial io_stream** _PL_streams();

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_write_term([NativeTypeName("IOSTREAM *")] io_stream* s, [NativeTypeName("term_t")] nuint term, int precedence, int flags);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_ttymode([NativeTypeName("IOSTREAM *")] io_stream* s);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_put_term_from_chars([NativeTypeName("term_t")] nuint t, int flags, [NativeTypeName("size_t")] nuint len, [NativeTypeName("const char *")] sbyte* s);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_chars_to_term([NativeTypeName("const char *")] sbyte* chars, [NativeTypeName("term_t")] nuint term);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_wchars_to_term([NativeTypeName("const pl_wchar_t *")] ushort* chars, [NativeTypeName("term_t")] nuint term);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_initialise(int argc, [NativeTypeName("char **")] sbyte** argv);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_winitialise(int argc, [NativeTypeName("wchar_t **")] ushort** argv);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_is_initialised(int* argc, [NativeTypeName("char ***")] sbyte*** argv);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_set_resource_db_mem([NativeTypeName("const unsigned char *")] byte* data, [NativeTypeName("size_t")] nuint size);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_toplevel();

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_cleanup(int status);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial void PL_cleanup_fork();

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_halt(int status);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial void* PL_dlopen([NativeTypeName("const char *")] sbyte* file, int flags);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    [return: NativeTypeName("const char *")]
    public static partial sbyte* PL_dlerror();

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial void* PL_dlsym(void* handle, [NativeTypeName("char *")] sbyte* symbol);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_dlclose(void* handle);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_dispatch(int fd, int wait);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial void PL_add_to_protocol([NativeTypeName("const char *")] sbyte* buf, [NativeTypeName("size_t")] nuint count);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    [return: NativeTypeName("char *")]
    public static partial sbyte* PL_prompt_string(int fd);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial void PL_write_prompt(int dowrite);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial void PL_prompt_next(int fd);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    [return: NativeTypeName("char *")]
    public static partial sbyte* PL_atom_generator([NativeTypeName("const char *")] sbyte* prefix, int state);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    [return: NativeTypeName("pl_wchar_t *")]
    public static partial ushort* PL_atom_generator_w([NativeTypeName("const pl_wchar_t *")] ushort* pref, [NativeTypeName("pl_wchar_t *")] ushort* buffer, [NativeTypeName("size_t")] nuint buflen, int state);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial void* PL_malloc([NativeTypeName("size_t")] nuint size);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial void* PL_malloc_atomic([NativeTypeName("size_t")] nuint size);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial void* PL_malloc_uncollectable([NativeTypeName("size_t")] nuint size);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial void* PL_malloc_atomic_uncollectable([NativeTypeName("size_t")] nuint size);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial void* PL_realloc(void* mem, [NativeTypeName("size_t")] nuint size);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial void* PL_malloc_unmanaged([NativeTypeName("size_t")] nuint size);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial void* PL_malloc_atomic_unmanaged([NativeTypeName("size_t")] nuint size);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial void PL_free(void* mem);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_linger(void* mem);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    [return: NativeTypeName("PL_dispatch_hook_t")]
    public static partial delegate* unmanaged[Cdecl]<int, int> PL_dispatch_hook([NativeTypeName("PL_dispatch_hook_t")] delegate* unmanaged[Cdecl]<int, int> param0);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial void PL_abort_hook([NativeTypeName("PL_abort_hook_t")] delegate* unmanaged[Cdecl]<void> param0);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial void PL_initialise_hook([NativeTypeName("PL_initialise_hook_t")] delegate* unmanaged[Cdecl]<int, sbyte**, void> param0);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_abort_unhook([NativeTypeName("PL_abort_hook_t")] delegate* unmanaged[Cdecl]<void> param0);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    [return: NativeTypeName("PL_agc_hook_t")]
    public static partial delegate* unmanaged[Cdecl]<nuint, int> PL_agc_hook([NativeTypeName("PL_agc_hook_t")] delegate* unmanaged[Cdecl]<nuint, int> param0);

    [DllImport("swipl", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern int PL_scan_options([NativeTypeName("term_t")] nuint options, int flags, [NativeTypeName("const char *")] sbyte* opttype, [NativeTypeName("PL_option_t[]")] PL_option_t* specs, __arglist);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    [return: NativeTypeName("void (*)(int)")]
    public static partial delegate* unmanaged[Cdecl]<int, void> PL_signal(int sig, [NativeTypeName("void (*)(int)")] delegate* unmanaged[Cdecl]<int, void> func);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_sigaction(int sig, [NativeTypeName("pl_sigaction_t *")] pl_sigaction* act, [NativeTypeName("pl_sigaction_t *")] pl_sigaction* old);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial void PL_interrupt(int sig);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_raise(int sig);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_handle_signals();

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_get_signum_ex([NativeTypeName("term_t")] nuint sig, int* n);

    [DllImport("swipl", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern int PL_action(int param0, __arglist);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial void PL_on_halt([NativeTypeName("int (*)(int, void *)")] delegate* unmanaged[Cdecl]<int, void*, int> param0, void* param1);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial void PL_exit_hook([NativeTypeName("int (*)(int, void *)")] delegate* unmanaged[Cdecl]<int, void*, int> param0, void* param1);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial void PL_backtrace(int depth, int flags);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    [return: NativeTypeName("char *")]
    public static partial sbyte* PL_backtrace_string(int depth, int flags);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_check_data([NativeTypeName("term_t")] nuint data);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_check_stacks();

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_current_prolog_flag([NativeTypeName("atom_t")] nuint name, int type, void* ptr);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    [return: NativeTypeName("unsigned int")]
    public static partial uint PL_version_info(int which);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    [return: NativeTypeName("intptr_t")]
    public static partial nint PL_query(int param0);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_thread_self();

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_unify_thread_id([NativeTypeName("term_t")] nuint t, int i);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_get_thread_id_ex([NativeTypeName("term_t")] nuint t, int* idp);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_get_thread_alias(int tid, [NativeTypeName("atom_t *")] nuint* alias);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_thread_attach_engine(PL_thread_attr_t* attr);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_thread_destroy_engine();

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_thread_at_exit([NativeTypeName("void (*)(void *)")] delegate* unmanaged[Cdecl]<void*, void> function, void* closure, int global);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_thread_raise(int tid, int sig);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    [return: NativeTypeName("PL_engine_t")]
    public static partial __PL_PL_local_data* PL_create_engine(PL_thread_attr_t* attributes);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_set_engine([NativeTypeName("PL_engine_t")] __PL_PL_local_data* engine, [NativeTypeName("PL_engine_t *")] __PL_PL_local_data** old);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_destroy_engine([NativeTypeName("PL_engine_t")] __PL_PL_local_data* engine);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    [return: NativeTypeName("hash_table_t")]
    public static partial __PL_table* PL_new_hash_table(int size, [NativeTypeName("void (*)(void *, void *)")] delegate* unmanaged[Cdecl]<void*, void*, void> free_symbol);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_free_hash_table([NativeTypeName("hash_table_t")] __PL_table* table);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial void* PL_lookup_hash_table([NativeTypeName("hash_table_t")] __PL_table* table, void* key);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial void* PL_add_hash_table([NativeTypeName("hash_table_t")] __PL_table* table, void* key, void* value, int flags);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial void* PL_del_hash_table([NativeTypeName("hash_table_t")] __PL_table* table, void* key);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_clear_hash_table([NativeTypeName("hash_table_t")] __PL_table* table);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    [return: NativeTypeName("hash_table_enum_t")]
    public static partial __PL_table_enum* PL_new_hash_table_enum([NativeTypeName("hash_table_t")] __PL_table* table);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial void PL_free_hash_table_enum([NativeTypeName("hash_table_enum_t")] __PL_table_enum* e);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_advance_hash_table_enum([NativeTypeName("hash_table_enum_t")] __PL_table_enum* e, void** key, void** value);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_register_profile_type(PL_prof_type_t* type);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial void* PL_prof_call(void* handle, PL_prof_type_t* type);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial void PL_prof_exit(void* node);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int emacs_module_init(void* param0);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_prolog_debug([NativeTypeName("const char *")] sbyte* topic);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_prolog_nodebug([NativeTypeName("const char *")] sbyte* topic);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int _PL_get_xpce_reference([NativeTypeName("term_t")] nuint t, xpceref_t* @ref);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int _PL_unify_xpce_reference([NativeTypeName("term_t")] nuint t, xpceref_t* @ref);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int _PL_put_xpce_reference_i([NativeTypeName("term_t")] nuint t, [NativeTypeName("uintptr_t")] nuint r);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int _PL_put_xpce_reference_a([NativeTypeName("term_t")] nuint t, [NativeTypeName("atom_t")] nuint name);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_get_context([NativeTypeName("struct pl_context_t *")] pl_context_t* c, int thead_id);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_step_context([NativeTypeName("struct pl_context_t *")] pl_context_t* c);

    [LibraryImport("swipl")]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int PL_describe_context([NativeTypeName("struct pl_context_t *")] pl_context_t* c, [NativeTypeName("char *")] sbyte* buf, [NativeTypeName("size_t")] nuint len);
}

using System.Runtime.InteropServices;
using System.Text;
using Prolog.NET.Swipl.Generated;

namespace Prolog.NET.Swipl;

/// <summary>
/// Represents a SWI-Prolog term bound in a query solution. The term reference is valid
/// only while the originating <see cref="PrologQuery"/> has not been disposed.
/// </summary>
public sealed class PrologTerm
{
    // term_t is a nuint handle — opaque to the public API.
    private readonly nuint _termRef;

    internal PrologTerm(nuint termRef)
    {
        _termRef = termRef;
    }

    /// <summary>True if the term is an unbound variable.</summary>
    public bool IsUnbound
    {
        get { unsafe { return SwiPrologNative.PL_is_variable(_termRef) != 0; } }
    }

    /// <summary>True if the term is a Prolog atom.</summary>
    public bool IsAtom
    {
        get { unsafe { return SwiPrologNative.PL_is_atom(_termRef) != 0; } }
    }

    /// <summary>True if the term is an integer.</summary>
    public bool IsInteger
    {
        get { unsafe { return SwiPrologNative.PL_is_integer(_termRef) != 0; } }
    }

    /// <summary>True if the term is a floating-point number.</summary>
    public bool IsFloat
    {
        get { unsafe { return SwiPrologNative.PL_is_float(_termRef) != 0; } }
    }

    /// <summary>True if the term is a compound term (functor with arguments).</summary>
    public bool IsCompound
    {
        get { unsafe { return SwiPrologNative.PL_is_compound(_termRef) != 0; } }
    }

    /// <summary>True if the term is a proper list.</summary>
    public bool IsList
    {
        get { unsafe { return SwiPrologNative.PL_is_list(_termRef) != 0; } }
    }

    /// <summary>
    /// Returns the atom name of this term.
    /// </summary>
    /// <exception cref="PrologException">Thrown if the term is not an atom.</exception>
    public string AsAtom()
    {
        unsafe
        {
            sbyte* chars;
            if (SwiPrologNative.PL_get_atom_chars(_termRef, &chars) == 0)
            {
                throw new PrologException("Term is not an atom");
            }

            return Marshal.PtrToStringUTF8((nint)chars)
                ?? throw new PrologException("Failed to marshal atom string");
        }
    }

    /// <summary>
    /// Returns the integer value of this term.
    /// </summary>
    /// <exception cref="PrologException">Thrown if the term is not an integer.</exception>
    public long AsInteger()
    {
        unsafe
        {
            long value;
            if (SwiPrologNative.PL_get_int64(_termRef, &value) == 0)
            {
                throw new PrologException("Term is not an integer");
            }

            return value;
        }
    }

    /// <summary>
    /// Returns the floating-point value of this term.
    /// </summary>
    /// <exception cref="PrologException">Thrown if the term is not a float.</exception>
    public double AsFloat()
    {
        unsafe
        {
            double value;
            if (SwiPrologNative.PL_get_float(_termRef, &value) == 0)
            {
                throw new PrologException("Term is not a float");
            }

            return value;
        }
    }

    /// <summary>
    /// Converts this term to its Prolog text representation using <c>term_to_atom/2</c>.
    /// </summary>
    public override string ToString()
    {
        unsafe
        {
            // term_to_atom(+Term, -Atom)
            fixed (byte* name = "term_to_atom\0"u8)
            fixed (byte* module = "system\0"u8)
            {
                __PL_procedure* pred = SwiPrologNative.PL_predicate((sbyte*)name, 2, (sbyte*)module);
                nuint args = SwiPrologNative.PL_new_term_refs(2);

                // args+0 = input Term, args+1 = output Atom
                if (SwiPrologNative.PL_put_term(args, _termRef) == 0)
                {
                    return "<term>";
                }

                __PL_queryRef* qid = SwiPrologNative.PL_open_query(
                    null,
                    PrologNativeConstants.PL_Q_CATCH_EXCEPTION,
                    pred,
                    args);

                try
                {
                    int rc = SwiPrologNative.PL_next_solution(qid);
                    if (rc == 0)
                    {
                        return "<term>";
                    }

                    sbyte* atomChars;
                    nuint atomRef = args + 1;
                    if (SwiPrologNative.PL_get_atom_chars(atomRef, &atomChars) == 0)
                    {
                        return "<term>";
                    }

                    return Marshal.PtrToStringUTF8((nint)atomChars) ?? "<term>";
                }
                finally
                {
                    SwiPrologNative.PL_close_query(qid);
                }
            }
        }
    }
}

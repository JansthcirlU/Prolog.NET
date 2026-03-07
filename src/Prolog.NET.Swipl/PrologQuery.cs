using System.Runtime.InteropServices;
using System.Text;
using Prolog.NET.Swipl.Generated;

namespace Prolog.NET.Swipl;

/// <summary>
/// Represents an open SWI-Prolog query. Use <see cref="Solutions"/> to enumerate all
/// solutions, or call <see cref="Next"/> manually for imperative iteration.
/// </summary>
/// <remarks>
/// Always dispose the query when done. Undisposed queries will leak the foreign frame
/// and prevent the Prolog engine from reclaiming stack space.
/// </remarks>
public sealed class PrologQuery : IDisposable
{
    // The engine that owns the Prolog thread; used to marshal all PL_* calls.
    private readonly PrologEngine _engine;

    // Foreign frame anchors all term_t refs allocated for this query.
    private nuint _frame;

    // The open query handle — __PL_queryRef* stored as nuint to avoid unsafe in fields.
    // Stored as nint (pointer-sized) since __PL_queryRef* is a pointer.
    private nint _queryRefPtr;

    // Maps Prolog variable names to their term_t handles.
    private readonly Dictionary<string, nuint> _variables = [];

    private bool _disposed;
    private bool _queryClosed;

    /// <summary>
    /// <see langword="true"/> after <see cref="Next"/> returns <see langword="true"/> and the
    /// native engine signalled <c>PL_S_LAST</c> — meaning this was the final solution and no
    /// further call to <see cref="Next"/> is needed (the query is already exhausted).
    /// </summary>
    public bool IsLastSolution { get; private set; }

    /// <summary>
    /// The current solution after a successful call to <see cref="Next"/>.
    /// <see langword="null"/> before the first call or after the query is exhausted.
    /// </summary>
    public PrologSolution? Current { get; private set; }

    /// <summary>
    /// Enumerates all solutions for the query. Each element is a snapshot of the
    /// variable bindings at that solution point.
    /// </summary>
    public IEnumerable<PrologSolution> Solutions
    {
        get
        {
            while (Next())
            {
                yield return Current!;
            }
        }
    }

    internal PrologQuery(string goal, PrologEngine engine)
    {
        _engine = engine;

        // 1. Open a foreign frame so all term_t refs allocated below remain valid
        //    for the lifetime of this query object.
        _frame = SwiPrologNative.PL_open_foreign_frame();

        try
        {
            ParseGoalAndOpenQuery(goal);
        }
        catch
        {
            SwiPrologNative.PL_close_foreign_frame(_frame);
            _frame = 0;
            throw;
        }
    }

    private unsafe void ParseGoalAndOpenQuery(string goal)
    {
        // 2. Call atom_to_term(GoalAtom, Term, Bindings) to parse the goal string
        //    and retrieve a list of Name=Variable pairs for all variables in the goal.
        byte[] goalBytes = Encoding.UTF8.GetBytes(goal + "\0");

        fixed (byte* goalPtr = goalBytes)
        fixed (byte* atomToTermName = "atom_to_term\0"u8)
        fixed (byte* systemModule = "system\0"u8)
        {
            __PL_procedure* parseProc = SwiPrologNative.PL_predicate(
                (sbyte*)atomToTermName, 3, (sbyte*)systemModule);

            // args+0 = input atom, args+1 = output Term, args+2 = output Bindings
            nuint parseArgs = SwiPrologNative.PL_new_term_refs(3);

            if (SwiPrologNative.PL_put_atom_chars(parseArgs, (sbyte*)goalPtr) == 0)
            {
                throw new PrologException($"Failed to create atom from goal: {goal}");
            }

            __PL_queryRef* parseQid = SwiPrologNative.PL_open_query(
                null,
                PrologNativeConstants.PL_Q_CATCH_EXCEPTION | PrologNativeConstants.PL_Q_EXT_STATUS,
                parseProc,
                parseArgs);

            int parseRc = SwiPrologNative.PL_next_solution(parseQid);
            // PL_cut_query (not PL_close_query) preserves the bindings atom_to_term made
            // to parseArgs+1 (Term) and parseArgs+2 (Bindings), which we need below.
            _ = SwiPrologNative.PL_cut_query(parseQid);

            if (parseRc == PrologNativeConstants.PL_S_EXCEPTION || parseRc == PrologNativeConstants.PL_S_FALSE)
            {
                throw new PrologException($"Failed to parse Prolog goal: {goal}");
            }

            // 3. Walk the Bindings list: ['VarName'=VarRef, ...]
            //    args+1 = parsed Term, args+2 = Bindings list
            nuint bindingsList = parseArgs + 2;
            nuint headRef = SwiPrologNative.PL_new_term_ref();
            nuint tailRef = SwiPrologNative.PL_new_term_ref();
            nuint nameRef = SwiPrologNative.PL_new_term_ref();

            // Iterate the list; each element is '='(Name, Var)
            while (SwiPrologNative.PL_get_list(bindingsList, headRef, tailRef) != 0)
            {
                // Extract Name (arg 1) and Var (arg 2) from the '='/2 pair
                _ = SwiPrologNative.PL_get_arg(1, headRef, nameRef);

                // Allocate a fresh term_t for the variable so each entry is independent
                nuint varRef = SwiPrologNative.PL_new_term_ref();
                _ = SwiPrologNative.PL_get_arg(2, headRef, varRef);

                sbyte* nameChars;
                if (SwiPrologNative.PL_get_atom_chars(nameRef, &nameChars) != 0)
                {
                    string varName = Marshal.PtrToStringUTF8((nint)nameChars)
                        ?? throw new PrologException("Failed to read variable name");
                    _variables[varName] = varRef;
                }

                // Advance: make bindingsList point to the tail
                _ = SwiPrologNative.PL_put_term(bindingsList, tailRef);
            }

            // 4. Open the real query: call(Term)
            //    args+1 is the parsed goal term.
            nuint goalTerm = parseArgs + 1;
            nuint callArg = SwiPrologNative.PL_new_term_refs(1);

            if (SwiPrologNative.PL_put_term(callArg, goalTerm) == 0)
            {
                throw new PrologException("Failed to prepare goal term for execution");
            }

            fixed (byte* callName = "call\0"u8)
            fixed (byte* userModule = "user\0"u8)
            {
                __PL_procedure* callProc = SwiPrologNative.PL_predicate(
                    (sbyte*)callName, 1, (sbyte*)userModule);

                __PL_queryRef* queryRef = SwiPrologNative.PL_open_query(
                    null,
                    PrologNativeConstants.PL_Q_CATCH_EXCEPTION | PrologNativeConstants.PL_Q_EXT_STATUS,
                    callProc,
                    callArg);

                // Store the pointer as nint to avoid unsafe fields
                _queryRefPtr = (nint)queryRef;
            }
        }
    }

    /// <summary>
    /// Advances the query to the next solution.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if a solution was found and <see cref="Current"/> is populated;
    /// <see langword="false"/> if there are no more solutions.
    /// </returns>
    /// <exception cref="PrologException">Thrown if the Prolog engine raised an exception.</exception>
    /// <exception cref="ObjectDisposedException">Thrown if the query has been disposed.</exception>
    public bool Next()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_queryClosed)
        {
            return false;
        }

        bool result = false;
        _engine.RunOnPrologThread(() => { result = NextCore(); });
        return result;
    }

    private unsafe bool NextCore()
    {
        __PL_queryRef* queryRef = (Generated.__PL_queryRef*)_queryRefPtr;
        int rc = SwiPrologNative.PL_next_solution(queryRef);

        switch (rc)
        {
            case PrologNativeConstants.PL_S_TRUE:
                IsLastSolution = false;
                Current = MakeSnapshot();
                return true;

            case PrologNativeConstants.PL_S_LAST:
                IsLastSolution = true;
                Current = MakeSnapshot();                       // snapshot FIRST — bindings still live
                _ = SwiPrologNative.PL_close_query(queryRef);  // close after snapshot
                _queryClosed = true;
                return true;

            case PrologNativeConstants.PL_S_FALSE:
                Current = null;
                _ = SwiPrologNative.PL_close_query(queryRef);  // close before flag
                _queryClosed = true;
                return false;

            case PrologNativeConstants.PL_S_EXCEPTION:
                Current = null;
                string? exMsg = TryGetExceptionMessage(queryRef);
                _ = SwiPrologNative.PL_close_query(queryRef);
                _queryClosed = true;
                throw new PrologException("Prolog exception raised during query", exMsg);

            default:
                Current = null;
                _queryClosed = true;
                return false;
        }
    }

    // Eagerly evaluates all variable term_t refs to strings while still on the Prolog thread,
    // so callers on other threads get safe string values rather than raw term handles.
    private PrologSolution MakeSnapshot()
    {
        Dictionary<string, string> termStrings = new(_variables.Count);
        foreach ((string name, nuint termRef) in _variables)
        {
            termStrings[name] = PrologEngine.TryTermToString(termRef) ?? "<term>";
        }

        return new PrologSolution(_variables, termStrings);
    }

    private static unsafe string? TryGetExceptionMessage(Generated.__PL_queryRef* queryRef)
    {
        nuint exTerm = SwiPrologNative.PL_exception(queryRef);
        if (exTerm == 0)
        {
            return null;
        }

        // Convert exception term to string using term_to_atom/2 in a sub-frame.
        nuint frame = SwiPrologNative.PL_open_foreign_frame();
        try
        {
            fixed (byte* name = "term_to_atom\0"u8)
            fixed (byte* module = "system\0"u8)
            {
                __PL_procedure* pred = SwiPrologNative.PL_predicate((sbyte*)name, 2, (sbyte*)module);
                nuint convArgs = SwiPrologNative.PL_new_term_refs(2);

                if (SwiPrologNative.PL_put_term(convArgs, exTerm) == 0)
                {
                    return null;
                }

                __PL_queryRef* convQid = SwiPrologNative.PL_open_query(
                    null,
                    PrologNativeConstants.PL_Q_NODEBUG | PrologNativeConstants.PL_Q_CATCH_EXCEPTION,
                    pred,
                    convArgs);

                try
                {
                    if (SwiPrologNative.PL_next_solution(convQid) != 0)
                    {
                        sbyte* atomChars;
                        if (SwiPrologNative.PL_get_atom_chars(convArgs + 1, &atomChars) != 0)
                        {
                            return Marshal.PtrToStringUTF8((nint)atomChars);
                        }
                    }
                }
                finally
                {
                    _ = SwiPrologNative.PL_close_query(convQid);
                }
            }
        }
        finally
        {
            SwiPrologNative.PL_discard_foreign_frame(frame);
        }

        return null;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        // If the engine is already disposed, PL_cleanup has run (or is running) on the
        // Prolog thread and will tear down all queries automatically — skip explicit cleanup.
        if (_engine.IsDisposed)
        {
            return;
        }

        _engine.RunOnPrologThread(() =>
        {
            unsafe
            {
                if (!_queryClosed && _queryRefPtr != 0)
                {
                    __PL_queryRef* queryRef = (Generated.__PL_queryRef*)_queryRefPtr;
                    _ = SwiPrologNative.PL_close_query(queryRef);
                }

                if (_frame != 0)
                {
                    SwiPrologNative.PL_close_foreign_frame(_frame);
                    _frame = 0;
                }
            }
        });
    }
}

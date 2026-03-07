using System.Collections.Concurrent;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Text;
using Prolog.NET.Swipl.Generated;

namespace Prolog.NET.Swipl;

/// <summary>
/// Manages the lifecycle of the SWI-Prolog engine. Only one instance may exist per
/// process, matching SWI-Prolog's single-engine-per-process constraint.
/// </summary>
/// <remarks>
/// <para>
/// Use <see cref="Initialize"/> to start the engine and <see cref="Dispose"/> to shut it
/// down. The engine must be initialized before any Prolog operations are performed.
/// </para>
/// <para>
/// All SWI-Prolog foreign-interface calls are marshalled to a single dedicated thread that
/// owns the engine for its lifetime. This satisfies SWI-Prolog's thread-local-storage
/// requirement on Linux and macOS, and is equally correct on Windows.
/// </para>
/// <para>
/// Disposing the engine calls <c>PL_cleanup</c>, which shuts down the Prolog runtime.
/// Re-initializing after disposal is not supported by SWI-Prolog.
/// </para>
/// </remarks>
public sealed class PrologEngine : IDisposable
{
    private static PrologEngine? _instance;
    private static readonly Lock _lock = new();

    private readonly Thread _prologThread;
    private readonly BlockingCollection<Action> _workQueue = [];
    private bool _disposed;

    private PrologEngine(string[]? args)
    {
        using ManualResetEventSlim initDone = new(false);
        Exception? initError = null;

        _prologThread = new Thread(() =>
        {
            try
            {
                InitializeNative(args);
            }
            catch (Exception ex)
            {
                initError = ex;
                initDone.Set();
                return;
            }

            initDone.Set();

            foreach (Action work in _workQueue.GetConsumingEnumerable())
            {
                work();
            }

            unsafe { _ = SwiPrologNative.PL_cleanup(0); }
        })
        {
            IsBackground = true,
            Name = "SWI-Prolog"
        };
        _prologThread.Start();

        initDone.Wait();

        if (initError != null)
        {
            ExceptionDispatchInfo.Capture(initError).Throw();
        }
    }

    internal bool IsDisposed => _disposed;

    /// <summary>
    /// Gets whether the SWI-Prolog engine is currently initialized.
    /// </summary>
    public static bool IsInitialized
    {
        get
        {
            unsafe { return SwiPrologNative.PL_is_initialised(null, null) != 0; }
        }
    }

    /// <summary>
    /// Initializes the SWI-Prolog engine and returns the singleton instance.
    /// </summary>
    /// <param name="args">
    /// Optional command-line arguments passed to SWI-Prolog (e.g. <c>["--quiet"]</c>).
    /// A program name is prepended automatically. If <see langword="null"/>, only the
    /// program name is passed.
    /// </param>
    /// <returns>The singleton <see cref="PrologEngine"/> instance.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the engine has already been initialized and not yet disposed.
    /// </exception>
    /// <exception cref="PrologException">
    /// Thrown if SWI-Prolog fails to initialize (e.g. missing runtime).
    /// </exception>
    public static PrologEngine Initialize(string[]? args = null)
    {
        lock (_lock)
        {
            if (_instance is { _disposed: false })
            {
                throw new InvalidOperationException(
                    "The Prolog engine is already initialized. Dispose the existing instance first.");
            }

            _instance = new PrologEngine(args);
            return _instance;
        }
    }

    /// <summary>
    /// Runs <paramref name="work"/> on the dedicated Prolog thread, blocking the caller
    /// until it completes. Exceptions are captured and re-thrown on the calling thread.
    /// </summary>
    internal void RunOnPrologThread(Action work)
    {
        ThrowIfDisposed();

        if (Thread.CurrentThread == _prologThread)
        {
            work();
            return;
        }

        using ManualResetEventSlim done = new(false);
        Exception? error = null;

        try
        {
            _workQueue.Add(() =>
            {
                try { work(); }
                catch (Exception ex) { error = ex; }
                finally { done.Set(); }
            });
        }
        catch (InvalidOperationException)
        {
            throw new ObjectDisposedException(nameof(PrologEngine));
        }

        done.Wait();
        if (error != null)
        {
            ExceptionDispatchInfo.Capture(error).Throw();
        }
    }

    /// <inheritdoc cref="RunOnPrologThread(Action)"/>
    internal T RunOnPrologThread<T>(Func<T> work)
    {
        ThrowIfDisposed();

        if (Thread.CurrentThread == _prologThread)
        {
            return work();
        }

        using ManualResetEventSlim done = new(false);
        T result = default!;
        Exception? error = null;

        try
        {
            _workQueue.Add(() =>
            {
                try { result = work(); }
                catch (Exception ex) { error = ex; }
                finally { done.Set(); }
            });
        }
        catch (InvalidOperationException)
        {
            throw new ObjectDisposedException(nameof(PrologEngine));
        }

        done.Wait();
        if (error != null)
        {
            ExceptionDispatchInfo.Capture(error).Throw();
        }

        return result;
    }

    private static unsafe void InitializeNative(string[]? args)
    {
        // Skip if already initialized (e.g. by a host process).
        if (SwiPrologNative.PL_is_initialised(null, null) != 0)
        {
            return;
        }

        // SWI-Prolog requires argv[0] to be the program name.
        string[] fullArgs = args is { Length: > 0 }
            ? ["swipl", .. args]
            : ["swipl"];

        // Marshal string[] to a null-terminated array of UTF-8 C strings.
        nint[] nativePtrs = new nint[fullArgs.Length];
        try
        {
            for (int i = 0; i < fullArgs.Length; i++)
            {
                nativePtrs[i] = Marshal.StringToCoTaskMemUTF8(fullArgs[i]);
            }

            fixed (nint* ptrArray = nativePtrs)
            {
                int rc = SwiPrologNative.PL_initialise(fullArgs.Length, (sbyte**)ptrArray);
                if (rc == 0)
                {
                    throw new PrologException("Failed to initialize SWI-Prolog engine. " +
                        "Ensure the SWI-Prolog runtime (swipl.dll / libswipl) is on the path.");
                }
            }
        }
        finally
        {
            foreach (nint ptr in nativePtrs)
            {
                if (ptr != 0)
                {
                    Marshal.FreeCoTaskMem(ptr);
                }
            }
        }
    }

    /// <summary>
    /// Loads a Prolog source file using <c>consult/1</c>.
    /// </summary>
    /// <param name="path">Absolute or relative path to the <c>.pl</c> file.</param>
    /// <exception cref="PrologException">Thrown if the file cannot be loaded.</exception>
    public void LoadFile(string path)
    {
        RunOnPrologThread(() =>
        {
            string escaped = EscapeSingleQuotes(path);
            if (!CallCore($"consult('{escaped}')"))
            {
                throw new PrologException($"consult/1 failed for: {path}");
            }
        });
    }

    /// <summary>
    /// Calls a Prolog goal and returns whether it succeeded.
    /// </summary>
    /// <param name="goal">A Prolog goal string, e.g. <c>"assert(likes(bob, alice))"</c>.</param>
    /// <returns>
    /// <see langword="true"/> if the goal succeeded; <see langword="false"/> if it failed.
    /// </returns>
    /// <exception cref="PrologException">
    /// Thrown if the goal string cannot be parsed or if Prolog raises an exception.
    /// </exception>
    public bool Call(string goal) => RunOnPrologThread(() => CallCore(goal));

    /// <summary>
    /// Opens a Prolog query for the given goal. Variables in the goal are automatically
    /// extracted and accessible on each <see cref="PrologSolution"/> via the indexer.
    /// </summary>
    /// <param name="goal">
    /// A Prolog goal string, e.g. <c>"member(X, [1,2,3])"</c>.
    /// </param>
    /// <returns>
    /// A <see cref="PrologQuery"/> that must be disposed after use.
    /// </returns>
    /// <exception cref="PrologException">Thrown if the goal cannot be parsed.</exception>
    public PrologQuery OpenQuery(string goal) =>
        RunOnPrologThread(() => new PrologQuery(goal, this));

    /// <inheritdoc/>
    public void Dispose()
    {
        lock (_lock)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _instance = null;

            // Signal the work loop to stop; PL_cleanup runs at the end of the thread.
            _workQueue.CompleteAdding();
            _prologThread.Join();
        }
    }

    private unsafe bool CallCore(string goal)
    {
        byte[] goalBytes = Encoding.UTF8.GetBytes(goal + "\0");

        fixed (byte* goalPtr = goalBytes)
        {
            nuint termRef = SwiPrologNative.PL_new_term_ref();

            if (SwiPrologNative.PL_chars_to_term((sbyte*)goalPtr, termRef) == 0)
            {
                throw new PrologException($"Failed to parse Prolog goal: {goal}");
            }

            int rc = SwiPrologNative.PL_call(termRef, null);

            if (rc == 0)
            {
                nuint exTerm = SwiPrologNative.PL_exception(null);
                if (exTerm != 0)
                {
                    string? msg = TryTermToString(exTerm);
                    SwiPrologNative.PL_clear_exception();
                    throw new PrologException("Prolog exception during Call", msg);
                }

                return false;
            }

            return true;
        }
    }

    // --- Helpers ---

    private void ThrowIfDisposed() =>
        ObjectDisposedException.ThrowIf(_disposed, this);

    private static string EscapeSingleQuotes(string path) =>
        path.Replace("'", "\\'");

    internal static unsafe string? TryTermToString(nuint termRef)
    {
        nuint frame = SwiPrologNative.PL_open_foreign_frame();
        try
        {
            fixed (byte* name = "term_to_atom\0"u8)
            fixed (byte* module = "system\0"u8)
            {
                __PL_procedure* pred = SwiPrologNative.PL_predicate((sbyte*)name, 2, (sbyte*)module);
                nuint args = SwiPrologNative.PL_new_term_refs(2);

                if (SwiPrologNative.PL_put_term(args, termRef) == 0)
                {
                    return null;
                }

                __PL_queryRef* qid = SwiPrologNative.PL_open_query(
                    null,
                    PrologNativeConstants.PL_Q_NODEBUG | PrologNativeConstants.PL_Q_CATCH_EXCEPTION,
                    pred,
                    args);

                try
                {
                    if (SwiPrologNative.PL_next_solution(qid) != 0)
                    {
                        sbyte* chars;
                        if (SwiPrologNative.PL_get_atom_chars(args + 1, &chars) != 0)
                        {
                            return Marshal.PtrToStringUTF8((nint)chars);
                        }
                    }
                }
                finally
                {
                    _ = SwiPrologNative.PL_close_query(qid);
                }
            }
        }
        finally
        {
            SwiPrologNative.PL_discard_foreign_frame(frame);
        }

        return null;
    }
}

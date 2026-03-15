using System.Collections.Concurrent;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Channels;
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
/// FLI operations are dispatched to a pool of SWI-Prolog attached threads. The pool size
/// is controlled by the <c>PROLOG_ENGINE_THREADS</c> environment variable (default: 1).
/// Each <see cref="PrologQuery"/> leases one thread for its entire lifetime, ensuring all
/// FLI calls for that query execute on the same OS thread as required by SWI-Prolog.
/// Non-query operations (<see cref="Call"/>, <see cref="LoadFile"/>) lease a thread, use
/// it, and return it immediately.
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

    private readonly PrologWorkerThread[] _pool;
    private readonly Channel<PrologWorkerThread> _idleThreads;
    private bool _disposed;

    private PrologEngine(string[]? args)
    {
        int threadCount = int.TryParse(
            Environment.GetEnvironmentVariable("PROLOG_ENGINE_THREADS"), out int n) && n > 0 ? n : 1;

        _pool = new PrologWorkerThread[threadCount];
        _idleThreads = Channel.CreateUnbounded<PrologWorkerThread>();

        // Thread 0: the main Prolog thread — calls PL_initialise on startup.
        _pool[0] = new PrologWorkerThread(isMainThread: true, index: 0, args);
        _pool[0].Start();

        Exception? initError = _pool[0].WaitReady();
        if (initError != null)
        {
            ExceptionDispatchInfo.Capture(initError).Throw();
        }

        // Threads 1..N-1: non-main threads — call PL_thread_attach_engine on startup.
        // PL_initialise must have completed on thread 0 before these are started.
        for (int i = 1; i < threadCount; i++)
        {
            _pool[i] = new PrologWorkerThread(isMainThread: false, index: i, null);
            _pool[i].Start();
        }

        for (int i = 1; i < threadCount; i++)
        {
            initError = _pool[i].WaitReady();
            if (initError != null)
            {
                ExceptionDispatchInfo.Capture(initError).Throw();
            }
        }

        // All threads ready — populate the idle pool.
        foreach (PrologWorkerThread t in _pool)
        {
            _idleThreads.Writer.TryWrite(t);
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
    /// Leases an idle worker thread from the pool, blocking until one is available.
    /// The caller must return the thread via <see cref="ReturnThread"/> when done.
    /// </summary>
    internal PrologWorkerThread LeaseThread()
    {
        ThrowIfDisposed();
        return _idleThreads.Reader.ReadAsync().AsTask().GetAwaiter().GetResult();
    }

    /// <summary>
    /// Returns a previously leased thread to the idle pool.
    /// </summary>
    internal void ReturnThread(PrologWorkerThread thread)
    {
        _idleThreads.Writer.TryWrite(thread);
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
        PrologWorkerThread thread = LeaseThread();
        try
        {
            thread.Run(() =>
            {
                string escaped = EscapeSingleQuotes(path);
                if (!CallCore($"consult('{escaped}')"))
                {
                    throw new PrologException($"consult/1 failed for: {path}");
                }
            });
        }
        finally { ReturnThread(thread); }
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
    public bool Call(string goal)
    {
        PrologWorkerThread thread = LeaseThread();
        try { return thread.Run(() => CallCore(goal)); }
        finally { ReturnThread(thread); }
    }

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
    public PrologQuery OpenQuery(string goal)
    {
        PrologWorkerThread thread = LeaseThread();
        // The query constructor runs on the leased thread and retains it for its lifetime.
        return thread.Run(() => new PrologQuery(goal, this, thread));
    }

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
        }

        // Signal non-main threads to drain and exit; they call PL_thread_destroy_engine.
        for (int i = 1; i < _pool.Length; i++)
        {
            _pool[i].CompleteAdding();
        }

        for (int i = 1; i < _pool.Length; i++)
        {
            _pool[i].Join();
        }

        // Queue PL_cleanup on the main Prolog thread (must run there), then shut it down.
        _pool[0].Run(() => { unsafe { _ = SwiPrologNative.PL_cleanup(0); } });
        _pool[0].CompleteAdding();
        _pool[0].Join();
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

    // -------------------------------------------------------------------------
    // PrologWorkerThread — an OS thread attached to the SWI-Prolog FLI.
    // -------------------------------------------------------------------------

    /// <summary>
    /// An OS thread permanently attached to the SWI-Prolog foreign-language interface.
    /// Thread 0 calls <c>PL_initialise</c>; threads 1..N call
    /// <c>PL_thread_attach_engine</c> / <c>PL_thread_destroy_engine</c>.
    /// </summary>
    internal sealed class PrologWorkerThread
    {
        private readonly BlockingCollection<Action> _queue = [];
        private readonly ManualResetEventSlim _readySignal = new(false);
        private Exception? _startError;

        public readonly Thread Thread;

        public PrologWorkerThread(bool isMainThread, int index, string[]? initArgs)
        {
            Thread = new Thread(() =>
            {
                try
                {
                    if (isMainThread)
                    {
                        InitializeNative(initArgs);
                    }
                    else
                    {
                        unsafe { _ = SwiPrologNative.PL_thread_attach_engine(null); }
                    }
                }
                catch (Exception ex)
                {
                    _startError = ex;
                    _readySignal.Set();
                    return;
                }

                _readySignal.Set();

                foreach (Action work in _queue.GetConsumingEnumerable())
                {
                    work();
                }

                if (!isMainThread)
                {
                    unsafe { _ = SwiPrologNative.PL_thread_destroy_engine(); }
                }
            })
            {
                IsBackground = true,
                Name = $"SWI-Prolog-{index}"
            };
        }

        public void Start() => Thread.Start();

        /// <summary>
        /// Blocks until the thread's startup (attach/initialise) completes.
        /// Returns the startup exception, or <see langword="null"/> on success.
        /// </summary>
        public Exception? WaitReady()
        {
            _readySignal.Wait();
            return _startError;
        }

        /// <summary>
        /// Posts <paramref name="work"/> to this thread's queue and blocks until it completes.
        /// </summary>
        public void Run(Action work)
        {
            using ManualResetEventSlim done = new(false);
            Exception? error = null;

            _queue.Add(() =>
            {
                try { work(); }
                catch (Exception ex) { error = ex; }
                finally { done.Set(); }
            });

            done.Wait();

            if (error != null)
            {
                ExceptionDispatchInfo.Capture(error).Throw();
            }
        }

        /// <inheritdoc cref="Run(Action)"/>
        public T Run<T>(Func<T> work)
        {
            using ManualResetEventSlim done = new(false);
            T result = default!;
            Exception? error = null;

            _queue.Add(() =>
            {
                try { result = work(); }
                catch (Exception ex) { error = ex; }
                finally { done.Set(); }
            });

            done.Wait();

            if (error != null)
            {
                ExceptionDispatchInfo.Capture(error).Throw();
            }

            return result;
        }

        /// <summary>Signals the thread to stop accepting new work after draining its queue.</summary>
        public void CompleteAdding() => _queue.CompleteAdding();

        /// <summary>Blocks until the thread has exited.</summary>
        public void Join() => Thread.Join();
    }
}

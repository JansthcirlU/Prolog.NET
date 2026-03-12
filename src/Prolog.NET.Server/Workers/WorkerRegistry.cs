using Microsoft.Extensions.Hosting;
using Proto;
using Prolog.NET.Actors;
using SystemProcess = System.Diagnostics.Process;
using SystemProcessStartInfo = System.Diagnostics.ProcessStartInfo;

namespace Prolog.NET.Server.Workers;

/// <summary>
/// Manages worker processes and routes Prolog queries to them.
/// </summary>
/// <remarks>
/// <para>
/// The registry maps each file path to one or more <see cref="WorkerEntry"/> instances.
/// When a query arrives for a file, the registry finds a worker with available capacity.
/// If none exists (or all workers for the file are at capacity), it spawns a new worker
/// process for that file and retries.
/// </para>
/// <para>
/// Capacity per worker is determined by <c>PROLOG_ENGINE_THREADS</c> (defaults to 1),
/// which mirrors the same env var read by <c>PrologWorkerActor</c>. When the worker
/// responds <c>Failed { "No capacity" }</c>, a new worker process is spawned.
/// </para>
/// </remarks>
public sealed class WorkerRegistry : IHostedService, IDisposable
{
    private sealed class WorkerEntry
    {
        public required PID Pid { get; init; }
        public required SystemProcess Process { get; init; }
        public int ActiveQueryCount { get; set; }
    }

    private readonly object _sync = new();
    private readonly Dictionary<string, List<WorkerEntry>> _workersByFile = [];
    private readonly Dictionary<string, (string FilePath, PID WorkerPid)> _activeQueries = [];
    private readonly ActorSystem _actorSystem;
    private readonly int _maxQueriesPerWorker;
    private int _nextPort = 4001;

    public WorkerRegistry(ActorSystem actorSystem)
    {
        _actorSystem = actorSystem;
        _maxQueriesPerWorker = int.TryParse(
            Environment.GetEnvironmentVariable("PROLOG_ENGINE_THREADS"), out int n) && n > 0 ? n : 1;
    }

    // --- IHostedService ---

    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StopAsync(CancellationToken cancellationToken)
    {
        lock (_sync)
        {
            foreach (List<WorkerEntry> workers in _workersByFile.Values)
            {
                foreach (WorkerEntry w in workers)
                {
                    try { w.Process.Kill(); w.Process.Dispose(); } catch { }
                }
            }

            _workersByFile.Clear();
            _activeQueries.Clear();
        }

        return Task.CompletedTask;
    }

    public void Dispose() { }

    // --- Public API ---

    /// <summary>
    /// Opens a streaming query for <paramref name="goal"/> against <paramref name="filePath"/>.
    /// Spawns a new worker process if no existing worker has capacity.
    /// </summary>
    /// <returns>
    /// On success: <c>(queryId, null)</c>. On failure: <c>(null, errorMessage)</c>.
    /// </returns>
    public async Task<(string? QueryId, string? Error)> OpenQueryAsync(
        string filePath, string goal, CancellationToken ct = default)
    {
        // Try each existing worker for this file.
        foreach (WorkerEntry worker in GetWorkersSnapshot(filePath))
        {
            if (worker.ActiveQueryCount >= _maxQueriesPerWorker)
            {
                continue;
            }

            OpenQueryResponse resp = await TryOpenQueryOnWorkerAsync(worker.Pid, goal, ct);

            if (resp.ResultCase == OpenQueryResponse.ResultOneofCase.Opened)
            {
                string queryId = resp.Opened.QueryId;
                lock (_sync)
                {
                    worker.ActiveQueryCount++;
                    _activeQueries[queryId] = (filePath, worker.Pid);
                }

                return (queryId, null);
            }

            if (resp.ResultCase == OpenQueryResponse.ResultOneofCase.Failed
                && !resp.Failed.Error.Contains("No capacity"))
            {
                return (null, resp.Failed.Error);
            }

            // "No capacity" — continue to next worker.
        }

        // No worker had capacity — spawn a new one.
        WorkerEntry? newWorker = await SpawnWorkerAsync(filePath, ct);
        if (newWorker == null)
        {
            return (null, "Failed to spawn worker process");
        }

        OpenQueryResponse newResp = await TryOpenQueryOnWorkerAsync(newWorker.Pid, goal, ct);

        if (newResp.ResultCase != OpenQueryResponse.ResultOneofCase.Opened)
        {
            return (null, newResp.Failed.Error);
        }

        string newQueryId = newResp.Opened.QueryId;
        lock (_sync)
        {
            newWorker.ActiveQueryCount++;
            _activeQueries[newQueryId] = (filePath, newWorker.Pid);
        }

        return (newQueryId, null);
    }

    /// <summary>
    /// Fetches the next solution for an open query.
    /// </summary>
    public async Task<NextSolutionResponse?> NextSolutionAsync(string queryId, CancellationToken ct = default)
    {
        PID? workerPid;
        lock (_sync)
        {
            workerPid = _activeQueries.TryGetValue(queryId, out (string FilePath, PID WorkerPid) info)
                ? info.WorkerPid
                : null;
        }

        if (workerPid == null)
        {
            return new NextSolutionResponse { NoMore = new NoMoreSolutionsResult() };
        }

        using CancellationTokenSource cts = new(TimeSpan.FromSeconds(30));
        using CancellationTokenSource linked = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, ct);

        NextSolutionResponse response = await _actorSystem.Root.RequestAsync<NextSolutionResponse>(
            workerPid, new NextSolutionMessage { QueryId = queryId }, linked.Token);

        bool done = response.ResultCase is
            NextSolutionResponse.ResultOneofCase.FinalSolution or
            NextSolutionResponse.ResultOneofCase.NoMore or
            NextSolutionResponse.ResultOneofCase.Failed;

        if (done)
        {
            DecrementAndRemove(queryId);
        }

        return response;
    }

    /// <summary>
    /// Closes an open query and releases its slot on the worker.
    /// </summary>
    public Task CloseQueryAsync(string queryId)
    {
        PID? workerPid;
        lock (_sync)
        {
            if (!_activeQueries.TryGetValue(queryId, out (string FilePath, PID WorkerPid) info))
            {
                return Task.CompletedTask;
            }

            workerPid = info.WorkerPid;
            DecrementAndRemove(queryId);
        }

        if (workerPid != null)
        {
            _actorSystem.Root.Send(workerPid, new CloseQueryMessage { QueryId = queryId });
        }

        return Task.CompletedTask;
    }

    // --- Helpers ---

    private List<WorkerEntry> GetWorkersSnapshot(string filePath)
    {
        lock (_sync)
        {
            return _workersByFile.TryGetValue(filePath, out List<WorkerEntry>? workers)
                ? [.. workers]
                : [];
        }
    }

    private async Task<OpenQueryResponse> TryOpenQueryOnWorkerAsync(
        PID workerPid, string goal, CancellationToken ct)
    {
        using CancellationTokenSource cts = new(TimeSpan.FromSeconds(15));
        using CancellationTokenSource linked = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, ct);

        return await _actorSystem.Root.RequestAsync<OpenQueryResponse>(
            workerPid, new OpenQueryMessage { Goal = goal }, linked.Token);
    }

    /// <summary>
    /// Spawns a new worker process for <paramref name="filePath"/>, waits for it to start,
    /// loads the file, and registers the worker in the registry.
    /// </summary>
    private async Task<WorkerEntry?> SpawnWorkerAsync(string filePath, CancellationToken ct)
    {
        int port;
        lock (_sync)
        {
            port = ++_nextPort;
        }

        SystemProcess? process = StartWorkerProcess(port);
        if (process == null)
        {
            return null;
        }

        // Give the worker's gRPC listener time to start.
        await Task.Delay(1500, ct);

        PID pid = PID.FromAddress($"127.0.0.1:{port}", "prolog");

        try
        {
            using CancellationTokenSource cts = new(TimeSpan.FromSeconds(15));
            CallResult loadResult = await _actorSystem.Root.RequestAsync<CallResult>(
                pid, new LoadFileMessage { Path = filePath }, cts.Token);

            if (!loadResult.Success)
            {
                try { process.Kill(); process.Dispose(); } catch { }
                return null;
            }
        }
        catch
        {
            try { process.Kill(); process.Dispose(); } catch { }
            return null;
        }

        WorkerEntry entry = new() { Pid = pid, Process = process };
        lock (_sync)
        {
            if (!_workersByFile.TryGetValue(filePath, out List<WorkerEntry>? workers))
            {
                workers = [];
                _workersByFile[filePath] = workers;
            }

            workers.Add(entry);
        }

        return entry;
    }

    private void DecrementAndRemove(string queryId)
    {
        lock (_sync)
        {
            if (!_activeQueries.Remove(queryId, out (string FilePath, PID WorkerPid) info))
            {
                return;
            }

            if (_workersByFile.TryGetValue(info.FilePath, out List<WorkerEntry>? workers))
            {
                WorkerEntry? worker = workers.FirstOrDefault(w => w.Pid.Equals(info.WorkerPid));
                if (worker != null)
                {
                    worker.ActiveQueryCount = Math.Max(0, worker.ActiveQueryCount - 1);
                }
            }
        }
    }

    private static SystemProcess? StartWorkerProcess(int port)
    {
        string workerExe = Environment.GetEnvironmentVariable("PROLOG_WORKER_EXE")
            ?? Path.Combine(AppContext.BaseDirectory, "Prolog.NET.Worker");

        if (OperatingSystem.IsWindows())
        {
            workerExe += ".exe";
        }

        SystemProcessStartInfo psi = new(workerExe, $"--port {port}")
        {
            UseShellExecute = false,
        };

        return SystemProcess.Start(psi);
    }
}

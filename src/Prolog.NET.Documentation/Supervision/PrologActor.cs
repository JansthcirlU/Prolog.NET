using Mermaid.Flowcharts.Nodes;

namespace Prolog.NET.Documentation.Supervision;

internal record PrologActor(Guid RequestId)
{
    internal Node ToNode(Guid pid, int workerIndex, int index)
        => Node.Create($"worker_{pid}_{workerIndex}_actor{index}", $"Actor {index} (Request {RequestId.ToString()[..8]})");

    internal Node ToEngineNode(Guid pid, int workerIndex, int index)
        => Node.Create($"worker_{pid}_{workerIndex}_actor{index}_engine", $"**PL_engine_t** for {RequestId.ToString()[..8]}");
}

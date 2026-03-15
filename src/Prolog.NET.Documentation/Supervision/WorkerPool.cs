using Mermaid.Flowcharts.Subgraphs;

namespace Prolog.NET.Documentation.Supervision;

internal record WorkerPool(int Capacity, Dictionary<string, List<PrologWorker>> ActiveWorkers)
{
    internal Subgraph ToSubgraph()
    {
        Subgraph subgraph = Subgraph.Create("worker_pool", $"Worker Pool ({ActiveWorkers.Sum(kvp => kvp.Value.Count)} / {Capacity})", SubgraphDirection.LR);
        foreach ((string key, List<PrologWorker> workers) in ActiveWorkers)
        {
            string fileName = Path.GetFileNameWithoutExtension(key);
            Subgraph fileWorkersSubgraph = Subgraph.Create($"workers_{fileName}_pl", $"**{fileName}.pl** workers", SubgraphDirection.LR);
            foreach ((PrologWorker worker, int index) in workers.Select((w, i) => (w, i + 1)))
            {
                fileWorkersSubgraph.AddNode(worker.ToSubgraph(index));
            }
            subgraph.AddNode(fileWorkersSubgraph);
        }
        return subgraph;
    }
}

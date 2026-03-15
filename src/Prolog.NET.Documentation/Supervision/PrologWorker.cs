using Mermaid.Flowcharts.Links;
using Mermaid.Flowcharts.Nodes;
using Mermaid.Flowcharts.Subgraphs;

namespace Prolog.NET.Documentation.Supervision;

internal record PrologWorker(Guid PID, string File, ActorPool ActorPool)
{
    internal Subgraph ToSubgraph(int index)
    {
        string fileName = Path.GetFileNameWithoutExtension(File);
        Subgraph workerProcess = Subgraph.Create($"process_{PID}", $"Process {PID.ToString()[..8]}", SubgraphDirection.LR);
        Node worker = Node.Create($"worker_{PID}_{index}", $"Worker {index} for **{fileName}.pl**");
        Node workerswipl = Node.Create($"worker_{PID}_{index}_swipl", $"Worker {index} **SWI-Prolog**");
        Subgraph actorPool = ActorPool.ToSubgraph(PID, index);
        Link workerToActorPoolLink = Link.Create(worker, actorPool, LinkType.Create(arrowType: LinkArrowType.Arrow, direction: LinkDirection.Both), "Stream Solutions");
        Link workerswiplToActorPoolLink = Link.Create(workerswipl, actorPool, LinkType.Create(direction: LinkDirection.Both, thickness: LinkThickness.Dotted), "PL_create_engine / PL_destroy_engine");
        workerProcess
            .AddNode(worker)
            .AddNode(workerswipl)
            .AddNode(actorPool)
            .AddLink(workerToActorPoolLink)
            .AddLink(workerswiplToActorPoolLink);
        return workerProcess;
    }
}

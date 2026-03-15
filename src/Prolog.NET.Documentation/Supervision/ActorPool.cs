using Mermaid.Flowcharts.Links;
using Mermaid.Flowcharts.Nodes;
using Mermaid.Flowcharts.Subgraphs;

namespace Prolog.NET.Documentation.Supervision;

internal record ActorPool(int Capacity, List<PrologActor> ActiveActors)
{
    internal Subgraph ToSubgraph(Guid pid, int workerIndex)
    {
        Subgraph subgraph = Subgraph.Create($"worker_{pid}_{workerIndex}_actorpool", $"Worker {workerIndex} actors ({ActiveActors.Count} / {Capacity})");
        foreach ((PrologActor actor, int index) in ActiveActors.Select((a, i) => (a, i + 1)))
        {
            Node actorNode = actor.ToNode(pid, workerIndex, index);
            Node actorEngineNode = actor.ToEngineNode(pid, workerIndex, index);
            Link actorToEngineLink = Link.Create(actorNode, actorEngineNode, LinkType.Create(direction: LinkDirection.Both, thickness: LinkThickness.Dotted), "Query-Scoped");
            subgraph
                .AddNode(actorNode)
                .AddNode(actorEngineNode)
                .AddLink(actorToEngineLink);
        }
        return subgraph;
    }
}

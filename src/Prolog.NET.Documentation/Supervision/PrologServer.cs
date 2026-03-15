using Mermaid.Flowcharts;
using Mermaid.Flowcharts.Links;
using Mermaid.Flowcharts.Nodes;
using Mermaid.Flowcharts.Subgraphs;

namespace Prolog.NET.Documentation.Supervision;

internal record PrologServer(RestEndpoint Rest, GrpcEndpoint Grpc, WorkerPool WorkerPool)
{
    public Flowchart ToFlowchart()
    {
        // Initialize flowchart with title
        FlowchartTitle title = FlowchartTitle.FromString("Prolog Server Supervision Hierarchy");
        Flowchart flowchart = new(title, FlowchartDirection.LR);

        // Add external endpoints and link to Prolog Server
        Node restApi = Rest.ToNode();
        Node grpc = Grpc.ToNode();
        Node prologServer = Node.Create("prolog_server", "Prolog Server");
        Link restApiToPrologServer = Link.Create(restApi, prologServer, LinkType.Create(LinkArrowType.Arrow, LinkDirection.Both, LinkThickness.Normal), "Request / Response");
        Link grpcToPrologServer = Link.Create(grpc, prologServer, LinkType.Create(LinkArrowType.Arrow, LinkDirection.Both, LinkThickness.Normal), "Server Streaming");
        
        // Add worker pool subgraph
        Subgraph workers = WorkerPool.ToSubgraph();
        Link prologServerToWorkersLink = Link.Create(prologServer, workers, LinkType.Create(LinkArrowType.Arrow, LinkDirection.Both, LinkThickness.Thick), "Inter-Process Communication");
        
        // Finalize flowchart creation
        return flowchart
            .AddNode(restApi)
            .AddNode(grpc)
            .AddNode(prologServer)
            .AddNode(workers)
            .AddLink(restApiToPrologServer)
            .AddLink(grpcToPrologServer)
            .AddLink(prologServerToWorkersLink);
    }
}

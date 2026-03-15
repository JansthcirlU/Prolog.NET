using Mermaid.Flowcharts.Nodes;

namespace Prolog.NET.Documentation.Supervision;

internal record GrpcEndpoint
{
    private readonly Guid _id = Guid.NewGuid();

    internal Node ToNode()
        => Node.Create($"grpc_{_id}", "gRPC");
}

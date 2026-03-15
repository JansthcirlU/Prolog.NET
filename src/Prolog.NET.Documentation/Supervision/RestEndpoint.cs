using Mermaid.Flowcharts.Nodes;

namespace Prolog.NET.Documentation.Supervision;

internal record RestEndpoint
{
    private readonly Guid _id = Guid.NewGuid();

    internal Node ToNode()
        => Node.Create($"restapi_{_id}", "REST API");
}

using Mermaid.Flowcharts;
using Mermaid.Flowcharts.Links;
using Mermaid.Flowcharts.Nodes;
using Mermaid.Flowcharts.Styling;
using Mermaid.Flowcharts.Styling.Attributes;
using Mermaid.Flowcharts.Subgraphs;

namespace Prolog.NET.Documentation;

internal static class PrologNetFlowcharts
{
    internal static IEnumerable<Flowchart> GetFlowcharts()
    {
        yield return GetArchitectureFlowchart();
        yield return GetSupervisionHierarchyFlowchart();
    }

    private static Flowchart GetArchitectureFlowchart()
    {
        FlowchartTitle title = FlowchartTitle.FromString("Communication Overview");
        Flowchart architecture = new(title);

        // Prolog files / knowledge bases
        Node family = Node.Create("file_family", "family.pl");
        Node school = Node.Create("file_school", "school.pl");
        architecture
            .AddNode(family)
            .AddNode(school);
        
        // Communication protocols
        Node http = Node.Create("api_http", "HTTP");
        Node grpc = Node.Create("api_grpc", "gRPC");
        Node cli = Node.Create("api_cli", "CLI");
        Node intermediate = Node.Create("intermediate_comms", "Intermediate Communication Layer");

        // Link styles: one color for each request
        StyleClass httpQueryFamilyStyle = new(
            Stroke: new(Color: Color.FromHex("#0000ff"))
        );
        StyleClass httpQuerySchoolStyle = new(
            Stroke: new(Color: Color.FromHex("#00ff00"))
        );
        StyleClass grpcQueryFamilyStyle = new(
            Stroke: new(Color: Color.FromHex("#ff0000"))
        );
        StyleClass cliQuerySchoolStyle = new(
            Stroke: new(Color: Color.FromHex("#fffb00"))
        );

        // Correlation ID for each request
        Guid httpQueryFamilyId = Guid.NewGuid();
        Guid httpQuerySchoolId = Guid.NewGuid();
        Guid grpcQueryFamilyId = Guid.NewGuid();
        Guid cliQuerySchoolId = Guid.NewGuid();

        // Query from each communication type
        Link httpQueryFamily = Link.Create(http, intermediate, linkText: $"query **family.pl**:\nid = {httpQueryFamilyId}", linkStyle: httpQueryFamilyStyle);
        Link httpQuerySchool = Link.Create(http, intermediate, linkText: $"query **school.pl**:\nid = {httpQuerySchoolId}", linkStyle: httpQuerySchoolStyle);
        Link grpcQueryFamily = Link.Create(grpc, intermediate, linkText: $"query **family.pl**:\nid = {grpcQueryFamilyId}", linkStyle: grpcQueryFamilyStyle);
        Link cliQuerySchool = Link.Create(cli, intermediate, linkText: $"query **school.pl**:\nid = {cliQuerySchoolId}", linkStyle: cliQuerySchoolStyle);
        architecture
            .AddNode(http)
            .AddNode(grpc)
            .AddNode(cli)
            .AddNode(intermediate)
            .AddLink(httpQueryFamily)
            .AddLink(httpQuerySchool)
            .AddLink(grpcQueryFamily)
            .AddLink(cliQuerySchool);

        // Worker file access
        Node workerFamily = Node.Create("worker_family", "Worker for family.pl");
        Node workerSchool = Node.Create("worker_school", "Worker for school.pl");
        Link intermediateToFamily1 = Link.Create(intermediate, workerFamily, linkText: httpQueryFamilyId.ToString(), linkStyle: httpQueryFamilyStyle);
        Link intermediateToFamily2 = Link.Create(intermediate, workerFamily, linkText: grpcQueryFamilyId.ToString(), linkStyle: grpcQueryFamilyStyle);
        Link intermediateToSchool1 = Link.Create(intermediate, workerSchool, linkText: httpQuerySchoolId.ToString(), linkStyle: httpQuerySchoolStyle);
        Link intermediateToSchool2 = Link.Create(intermediate, workerSchool, linkText: cliQuerySchoolId.ToString(), linkStyle: cliQuerySchoolStyle);
        architecture
            .AddNode(workerFamily)
            .AddNode(workerSchool)
            .AddLink(intermediateToFamily1)
            .AddLink(intermediateToFamily2)
            .AddLink(intermediateToSchool1)
            .AddLink(intermediateToSchool2);

        // Actor orchestration
        Node actorFamily1 = Node.Create("actor_family1", "Actor 1 for family.pl");
        Node actorFamily2 = Node.Create("actor_family2", "Actor 2 for family.pl");
        Node actorSchool1 = Node.Create("actor_school1", "Actor 1 for school.pl");
        Node actorSchool2 = Node.Create("actor_school2", "Actor 2 for school.pl");
        Link workerFamilyToActorFamily1 = Link.Create(workerFamily, actorFamily1, linkText: httpQueryFamilyId.ToString(), linkStyle: httpQueryFamilyStyle);
        Link workerFamilyToActorFamily2 = Link.Create(workerFamily, actorFamily2, linkText: grpcQueryFamilyId.ToString(), linkStyle: grpcQueryFamilyStyle);
        Link workerSchoolToActorSchool1 = Link.Create(workerSchool, actorSchool1, linkText: httpQuerySchoolId.ToString(), linkStyle: httpQuerySchoolStyle);
        Link workerSchoolToActorSchool2 = Link.Create(workerSchool, actorSchool2, linkText: cliQuerySchoolId.ToString(), linkStyle: cliQuerySchoolStyle);
        architecture
            .AddNode(actorFamily1)
            .AddNode(actorFamily2)
            .AddNode(actorSchool1)
            .AddNode(actorSchool2)
            .AddLink(workerFamilyToActorFamily1)
            .AddLink(workerFamilyToActorFamily2)
            .AddLink(workerSchoolToActorSchool1)
            .AddLink(workerSchoolToActorSchool2);
        
        // File linking
        Link actorFamily1ToFamily = Link.Create(actorFamily1, family, linkText: httpQueryFamilyId.ToString(), linkStyle: httpQueryFamilyStyle);
        Link actorFamily2ToFamily = Link.Create(actorFamily2, family, linkText: grpcQueryFamilyId.ToString(), linkStyle: grpcQueryFamilyStyle);
        Link actorSchool1ToFamily = Link.Create(actorSchool1, school, linkText: httpQuerySchoolId.ToString(), linkStyle: httpQuerySchoolStyle);
        Link actorSchool2ToFamily = Link.Create(actorSchool2, school, linkText: cliQuerySchoolId.ToString(), linkStyle: cliQuerySchoolStyle);
        architecture
            .AddLink(actorFamily1ToFamily)
            .AddLink(actorFamily2ToFamily)
            .AddLink(actorSchool1ToFamily)
            .AddLink(actorSchool2ToFamily);

        return architecture;
    }

    private static Flowchart GetSupervisionHierarchyFlowchart()
    {
        FlowchartTitle title = FlowchartTitle.FromString("Supervision Hierarchy");
        Flowchart supervision = new(title);

        // Server layer
        Subgraph server = Subgraph.Create("server", "Server", SubgraphDirection.TB);
        Node router = Node.Create("router", "Message Router");
        server
            .AddNode(router);

        // Family workers layer
        Subgraph workersFamily = Subgraph.Create("workers_family", "Workers for **family.pl**", SubgraphDirection.TB);
        Link routerToWorkersFamilyLink = Link.Create(router, workersFamily, linkText: "**family.pl** communication");
        server
            .AddNode(workersFamily)
            .AddLink(routerToWorkersFamilyLink);
        
        Node workerFamily1 = Node.Create("worker_family1", "Worker 1");
        Subgraph workerFamily1Actors = Subgraph.Create("worker_family1_actors", "Worker 1 actors", SubgraphDirection.TB);
        Node workerFamily1Actor1 = Node.Create("worker_family1_actor1", "Actor 1");
        Node workerFamily1Actor2 = Node.Create("worker_family1_actor2", "Actor 2");
        Node workerFamily1Actor3 = Node.Create("worker_family1_actor3", "Actor 3");
        workerFamily1Actors
            .AddNode(workerFamily1Actor1)
            .AddNode(workerFamily1Actor2)
            .AddNode(workerFamily1Actor3);
        Link workerFamily1MaxCapacity = Link.Create(workerFamily1, workerFamily1Actors, linkText: "max capacity file worker");
        workersFamily
            .AddNode(workerFamily1)
            .AddNode(workerFamily1Actors)
            .AddLink(workerFamily1MaxCapacity);
        
        Node workerFamily2 = Node.Create("worker_family2", "Worker 2");
        Subgraph workerFamily2Actors = Subgraph.Create("worker_family2_actors", "Worker 2 actors", SubgraphDirection.TB);
        Node workerFamily2Actor1 = Node.Create("worker_family2_actor1", "Actor 1");
        Node workerFamily2Actor2 = Node.Create("worker_family2_actor2", "Actor 2");
        workerFamily2Actors
            .AddNode(workerFamily2Actor1)
            .AddNode(workerFamily2Actor2);
        Link workerFamily2Link = Link.Create(workerFamily2, workerFamily2Actors, linkText: "extra file worker");
        workersFamily
            .AddNode(workerFamily2)
            .AddNode(workerFamily2Actors)
            .AddLink(workerFamily2Link);
        
        // School workers layer
        Subgraph workersSchool = Subgraph.Create("workers_school", "Workers for **school.pl**", SubgraphDirection.TB);
        Link routerToWorkersSchoolLink = Link.Create(router, workersSchool, linkText: "**school.pl** communication");
        server
            .AddNode(workersSchool)
            .AddLink(routerToWorkersSchoolLink);

        Node workerSchool1 = Node.Create("worker_school1", "Worker 1");
        Subgraph workerSchool1Actors = Subgraph.Create("worker_school1_actors", "Worker 1 actors", SubgraphDirection.TB);
        Node workerSchool1Actor1 = Node.Create("worker_school1_actor1", "Actor 1");
        Node workerSchool1Actor2 = Node.Create("worker_school1_actor2", "Actor 2");
        workerSchool1Actors
            .AddNode(workerSchool1Actor1)
            .AddNode(workerSchool1Actor2);
        Link workerSchool1Link = Link.Create(workerSchool1, workerSchool1Actors, linkText: "file worker");
        workersSchool
            .AddNode(workerSchool1)
            .AddNode(workerSchool1Actors)
            .AddLink(workerSchool1Link);

        // Final flowchart
        supervision.AddNode(server);
        return supervision;
    }
}
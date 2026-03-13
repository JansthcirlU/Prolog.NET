namespace Prolog.NET.Documentation.Conceptual.Server.Rest;

internal abstract record RestRequest
{
    internal sealed record StartQueryRequest(Guid RequestId, string FileName, string Goal) : RestRequest;
    internal sealed record NextSolutionRequest(Guid RequestId, NextSolutionToken NextSolutionToken) : RestRequest;
    internal sealed record StopQueryRequest(Guid RequestId) : RestRequest;

    internal static StartQueryRequest StartQuery(string fileName, string goal)
        => new(Guid.NewGuid(), fileName, goal);

    internal static NextSolutionRequest NextSolution(Guid requestId, NextSolutionToken nextSolutionToken)
        => new(requestId, nextSolutionToken);

    internal static StopQueryRequest StopQuery(Guid requestId)
        => new(requestId);
}

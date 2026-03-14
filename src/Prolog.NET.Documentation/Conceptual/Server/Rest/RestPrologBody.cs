namespace Prolog.NET.Documentation.Conceptual.Server.Rest;

internal abstract record RestPrologBody(bool ShouldDispose)
{
    internal sealed record RequestStarted(Guid RequestId, NextSolutionToken NextSolutionToken) : RestPrologBody(false);
    internal sealed record RequestSolution(Guid RequestId, string Solution, NextSolutionToken? NextSolutionToken) : RestPrologBody(NextSolutionToken is null);
    internal sealed record RequestStopped(Guid RequestId, RequestStoppedReason Reason) : RestPrologBody(true);
    internal sealed record BadRequest(string Message, bool ShouldDispose) : RestPrologBody(ShouldDispose);
    internal abstract record PrologServerProblemDetails(string Type, string Title, int Status, string Detail, string Instance, bool ShouldDispose) : RestPrologBody(ShouldDispose)
    {
        internal sealed record PrologServerException(bool ShouldDispose) : PrologServerProblemDetails("https://www.rfc-editor.org/rfc/rfc9110.html#name-500-internal-server-error", "Prolog Server Exception", 500, "An exception occurred while handling the request.", "https://www.rfc-editor.org/rfc/rfc9110.html#name-500-internal-server-error", ShouldDispose);
        internal sealed record PrologNotImplemented(bool ShouldDispose) : PrologServerProblemDetails("https://www.rfc-editor.org/rfc/rfc9110.html#name-501-not-implemented", "Prolog Not Implemented Exception", 501, "The querier response type is currently not supported", "https://www.rfc-editor.org/rfc/rfc9110.html#name-501-not-implemented", ShouldDispose);
        internal sealed record PrologQuerierDisposing() : PrologServerProblemDetails("https://www.rfc-editor.org/rfc/rfc9110.html#name-500-internal-server-error", "Prolog Querier Dispoing Exception", 500, "The solution querier is being disposed at the time of requesting the next solution. This request ID will become invalid.", "https://www.rfc-editor.org/rfc/rfc9110.html#name-500-internal-server-error", true);
    }
}

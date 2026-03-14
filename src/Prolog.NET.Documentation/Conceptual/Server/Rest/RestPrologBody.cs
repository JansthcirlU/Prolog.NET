namespace Prolog.NET.Documentation.Conceptual.Server.Rest;

internal abstract record RestPrologBody(bool WillDispose)
{
    internal sealed record RequestStarted(Guid RequestId, NextSolutionToken NextSolutionToken) : RestPrologBody(false);
    internal sealed record RequestSolution(Guid RequestId, string Solution, NextSolutionToken? NextSolutionToken) : RestPrologBody(false);
    internal sealed record RequestStopped(Guid RequestId, RequestStoppedReason Reason) : RestPrologBody(true);
    internal sealed record BadRequest(string Message, bool WillDispose) : RestPrologBody(WillDispose);
    internal abstract record PrologServerProblemDetails(string Type, string Title, int Status, string Detail, string Instance, bool WillDispose) : RestPrologBody(WillDispose)
    {
        internal sealed record PrologServerException(bool WillDispose) : PrologServerProblemDetails("https://www.rfc-editor.org/rfc/rfc9110.html#name-500-internal-server-error", "Prolog Server Exception", 500, "An exception occurred while handling the request.", "https://www.rfc-editor.org/rfc/rfc9110.html#name-500-internal-server-error", WillDispose);
        internal sealed record PrologNotImplemented(bool WillDispose) : PrologServerProblemDetails("https://www.rfc-editor.org/rfc/rfc9110.html#name-501-not-implemented", "Prolog Not Implemented Exception", 501, "The querier response type is currently not supported", "https://www.rfc-editor.org/rfc/rfc9110.html#name-501-not-implemented", WillDispose);
        internal sealed record PrologQuerierDisposing() : PrologServerProblemDetails("https://www.rfc-editor.org/rfc/rfc9110.html#name-500-internal-server-error", "Prolog Querier Dispoing Exception", 500, "The solution querier is being disposed at the time of requesting the next solution. This request ID will become invalid.", "https://www.rfc-editor.org/rfc/rfc9110.html#name-500-internal-server-error", true);
    }
}

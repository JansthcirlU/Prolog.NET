namespace Prolog.NET.Documentation.Conceptual.Server.Rest;

internal abstract record RestPrologBody
{
    internal sealed record RequestStarted(Guid RequestId, NextSolutionToken NextSolutionToken) : RestPrologBody;
    internal sealed record RequestSolution(Guid RequestId, string Solution, NextSolutionToken? NextSolutionToken) : RestPrologBody;
    internal sealed record RequestStopped(Guid RequestId, RequestStoppedReason Reason) : RestPrologBody;
    internal sealed record BadRequest(string Message) : RestPrologBody;
    internal abstract record PrologServerProblemDetails(string Type, string Title, int Status, string Detail, string Instance) : RestPrologBody
    {
        internal sealed record PrologServerException() : PrologServerProblemDetails("https://www.rfc-editor.org/rfc/rfc9110.html#name-500-internal-server-error", "Prolog Server Exception", 500, "An exception occurred while handling the request.", "https://www.rfc-editor.org/rfc/rfc9110.html#name-500-internal-server-error");
        internal sealed record PrologNotImplemented() : PrologServerProblemDetails("https://www.rfc-editor.org/rfc/rfc9110.html#name-501-not-implemented", "Prolog Not Implemented Exception", 501, "The querier response type is currently not supported", "https://www.rfc-editor.org/rfc/rfc9110.html#name-501-not-implemented");
        internal sealed record PrologQuerierDisposed() : PrologServerProblemDetails("https://www.rfc-editor.org/rfc/rfc9110.html#name-500-internal-server-error", "Prolog Querier Disposed Exception", 500, "The solution querier was disposed while requesting the next solution.", "https://www.rfc-editor.org/rfc/rfc9110.html#name-500-internal-server-error");
    }
}

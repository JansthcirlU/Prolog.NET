using Prolog.NET.Documentation.Conceptual.Swipl;
using Prolog.NET.Documentation.Conceptual.Worker;

namespace Prolog.NET.Documentation.Conceptual.Server.Rest;

internal abstract record QuerierResponse
{
    internal sealed record NextResponse(PrologWorkerResponse PrologWorkerResponse, NextSolutionToken Token) : QuerierResponse;
    internal sealed record QueryExhaustedResponse : QuerierResponse;
    internal sealed record QuerierInterruptedResponse : QuerierResponse;
    internal sealed record InvalidTokenResponse(NextSolutionToken Token) : QuerierResponse;
    internal sealed record QuerierDisposedResponse : QuerierResponse;
    internal sealed record ExceptionResponse(Exception Exception) : QuerierResponse;
    internal sealed record PrologEngineExceptionResponse(PrologEngineException PrologEngineException) : QuerierResponse;
    internal sealed record PrologWorkerExceptionResponse(PrologWorkerException PrologWorkerException) : QuerierResponse;

    internal static NextResponse Next(PrologWorkerResponse prologWorkerResponse, NextSolutionToken token)
        => new(prologWorkerResponse, token);
    
    internal static QueryExhaustedResponse Exhausted()
        => new();
    
    internal static QuerierInterruptedResponse QuerierInterrupted()
        => new();
    
    internal static InvalidTokenResponse InvalidToken(NextSolutionToken token)
        => new(token);

    internal static QuerierDisposedResponse QuerierDisposed()
        => new();

    internal static ExceptionResponse FromException(Exception exception)
        => new(exception);
    
    internal static PrologEngineExceptionResponse FromEngineException(PrologEngineException prologEngineException)
        => new(prologEngineException);
    
    internal static PrologWorkerExceptionResponse FromWorkerException(PrologWorkerException prologWorkerException)
        => new(prologWorkerException);
}

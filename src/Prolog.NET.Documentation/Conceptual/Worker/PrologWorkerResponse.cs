using Prolog.NET.Documentation.Conceptual.Swipl;

namespace Prolog.NET.Documentation.Conceptual.Worker;

internal abstract record PrologWorkerResponse
{
    internal sealed record EngineResponse(PrologEngineResponse Response) : PrologWorkerResponse;
    internal sealed record ExceptionResponse(PrologWorkerException Exception) : PrologWorkerResponse;

    internal static EngineResponse FromEngine(PrologEngineResponse response)
        => new(response);
    
    internal static ExceptionResponse FromException(PrologWorkerException exception)
        => new(exception);
}

internal class PrologWorkerException : Exception
{
    internal PrologWorkerException(string? message) : base(message)
    {
        
    }
    internal PrologWorkerException(string? message, Exception innerException) : base(message, innerException)
    {
        
    }
}
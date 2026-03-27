namespace Prolog.NET.Documentation.Conceptual.Swipl;

internal abstract record PrologEngineResponse
{
    internal sealed record SolutionResponse(string Value) : PrologEngineResponse;
    internal sealed record FinalSolutionResponse(string Value) : PrologEngineResponse;
    internal sealed record QueryHaltedResponse : PrologEngineResponse;
    internal sealed record ExceptionResponse(PrologEngineException Exception) : PrologEngineResponse;

    internal static SolutionResponse Solution(string value)
        => new(value);

    internal static FinalSolutionResponse FinalSolution(string value)
        => new(value);

    internal static QueryHaltedResponse Halted()
        => new();

    internal static ExceptionResponse FromException(PrologEngineException exception)
        => new(exception);
}

internal abstract class PrologEngineException : Exception
{
    internal PrologEngineException(string? message) : base(message)
    {

    }

    internal sealed class EngineNotInitializedException : PrologEngineException
    {
        internal EngineNotInitializedException(string? message) : base(message)
        {
        }
    }

    internal sealed class InvalidQueryException : PrologEngineException
    {
        internal InvalidQueryException(string? message) : base(message)
        {
        }
    }

    internal sealed class MiscellaneousException : PrologEngineException
    {
        public bool RequestDisposal { get; }

        internal MiscellaneousException(string? message, bool requestDisposal) : base(message)
        {
            RequestDisposal = requestDisposal;
        }
    }


    internal static EngineNotInitializedException EngineNotInitialized(string? message)
        => new(message);

    internal static InvalidQueryException InvalidQuery(string? message)
        => new(message);

    internal static MiscellaneousException Miscellaneous(string? message, bool requestDisposal)
        => new(message, requestDisposal);
}

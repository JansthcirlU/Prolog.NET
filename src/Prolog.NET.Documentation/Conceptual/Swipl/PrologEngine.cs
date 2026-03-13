namespace Prolog.NET.Documentation.Conceptual.Swipl;

internal sealed class PrologEngine
{
    private readonly string _goal;
    private bool _hasStarted;
    private bool _isFaulted;
    private IEnumerator<PrologEngineResponse> _mockResponses;

    internal Guid Id { get; }

    internal PrologEngine(Guid id, string goal)
    {
        Id = id;
        _goal = goal;
        _hasStarted = false;
        _isFaulted = false;
        _mockResponses = MockResponses().GetEnumerator();
    }

    internal Task InitialiseAsync()
    {
        _hasStarted = true;
        if (string.IsNullOrWhiteSpace(_goal))
        {
            _isFaulted = true;
        }
        return Task.CompletedTask;
    }

    internal Task<PrologEngineResponse> GetNextResponseAsync(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromResult<PrologEngineResponse>(PrologEngineResponse.Halted());
        }
        if (!_mockResponses.MoveNext())
        {
            return Task.FromException<PrologEngineResponse>(new InvalidOperationException("No more solutions"));
        }
        PrologEngineResponse next = _mockResponses.Current;
        return Task.FromResult(next);
    }

    private IEnumerable<PrologEngineResponse> MockResponses(bool withError = false)
    {
        // Check if engine has started
        if (_hasStarted)
        {
            yield return PrologEngineResponse.FromException(PrologEngineException.EngineNotInitialized("Engine not initialised."));
            yield break;
        }

        // Check if query was valid
        if (_isFaulted)
        {
            yield return PrologEngineResponse.FromException(PrologEngineException.InvalidQuery("Invalid query."));
            yield break;
        }

        // Start enumerating solutions
        int max = 3;
        for (int i = 1; i < max; i++)
        {
            yield return PrologEngineResponse.Solution($"X = {i}");
        }

        // Interrupt solution stream with error
        if (withError)
        {
            yield return PrologEngineResponse.FromException(PrologEngineException.Miscellaneous("Something went wrong."));
            yield break;
        }

        // Yield final solution
        yield return PrologEngineResponse.FinalSolution($"X = {max}");
    }
}

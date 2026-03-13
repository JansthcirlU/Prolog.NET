using System.Security.Cryptography;
using Prolog.NET.Documentation.Conceptual.Worker;

namespace Prolog.NET.Documentation.Conceptual.Server.Rest;

internal record NextSolutionToken
{
    internal int Value { get; }

    private NextSolutionToken(int value)
    {
        Value = value;
    }

    internal static NextSolutionToken New()
        => new(RandomNumberGenerator.GetInt32(int.MaxValue));
}

internal sealed class RestSolutionQuerier : IAsyncDisposable
{
    private long _disposing;
    private readonly IAsyncEnumerator<PrologWorkerResponse> _inner;
    private readonly CancellationTokenSource _cancelEnumerator;
    private readonly Lock _lock;
    private NextSolutionToken _token;

    private RestSolutionQuerier(IAsyncEnumerator<PrologWorkerResponse> enumerator, CancellationTokenSource cancelEnumerator, NextSolutionToken token)
    {
        _disposing = 0;
        _inner = enumerator;
        _cancelEnumerator = cancelEnumerator;
        _lock = new();
        _token = token;
    }

    internal static RestSolutionQuerier CreateWithNextToken(IAsyncEnumerator<PrologWorkerResponse> solutions, CancellationTokenSource cancelEnumerator, out NextSolutionToken token)
    {
        token = NextSolutionToken.New();
        return new(solutions, cancelEnumerator, token);
    }

    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _disposing, 1) == 0)
        {
            await _cancelEnumerator.CancelAsync();
            _cancelEnumerator.Dispose();
            await _inner.DisposeAsync();
        }
    }

    internal async Task<QuerierResponse> GetNextSolutionAsync(NextSolutionToken token, CancellationToken cancellationToken)
    {
        if (Interlocked.Read(ref _disposing) == 0)
        {
            try
            {
                lock (_lock)
                {
                    if (_token.Value != token.Value)
                    {
                        return QuerierResponse.InvalidToken(token);
                    }
                    _token = NextSolutionToken.New();
                }
                bool canGetCurrent = Interlocked.Read(ref _disposing) == 0 && await _inner.MoveNextAsync();
                if (!canGetCurrent)
                {
                    return Interlocked.Read(ref _disposing) == 0
                        ? QuerierResponse.Exhausted()
                        : QuerierResponse.QuerierDisposed();
                }
                return QuerierResponse.Next(_inner.Current, _token);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return QuerierResponse.QuerierInterrupted();
            }
            catch (Exception ex)
            {
                return QuerierResponse.FromException(ex);
            }
        }
        else
        {
            return QuerierResponse.QuerierDisposed();
        }
    }
}

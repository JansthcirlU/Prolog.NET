using System.Collections.Concurrent;
using Prolog.NET.Documentation.Conceptual.Swipl;
using Prolog.NET.Documentation.Conceptual.Worker;

namespace Prolog.NET.Documentation.Conceptual.Server.Rest;

internal sealed class RestPrologServer
{
    private readonly ConcurrentDictionary<Guid, RestSolutionQuerier> _queryResponses;
    private readonly PrologServer _prologServer;

    internal RestPrologServer()
    {
        _queryResponses = [];
        _prologServer = PrologServer.Instance;
    }

    internal async Task<RestResponse> HandleRequestAsync(RestRequest request, CancellationToken cancellationToken)
        => await (request switch
        {
            RestRequest.StartQueryRequest startQuery => HandleStartQueryRequestAsync(startQuery, cancellationToken),
            RestRequest.NextSolutionRequest nextSolution => HandleNextSolutionRequestAsync(nextSolution, cancellationToken),
            RestRequest.StopQueryRequest stopQuery => HandleStopQueryRequestAsync(stopQuery, cancellationToken),
            _ => Task.FromResult<RestResponse>(new RestResponse.BadRequestResponse<RestPrologBody.BadRequest>(new("Invalid request.")))
        });

    private async Task<RestResponse> HandleStartQueryRequestAsync(RestRequest.StartQueryRequest startQuery, CancellationToken cancellationToken)
    {
        // Initialize solution querier
        CancellationTokenSource cancelEnumerator = new();
        IAsyncEnumerator<PrologWorkerResponse> solutions = _prologServer.QueryFileAsync(startQuery.FileName, startQuery.Goal, cancelEnumerator.Token).GetAsyncEnumerator(cancellationToken);
        Guid requestId = startQuery.RequestId;
        RestSolutionQuerier querier = RestSolutionQuerier.CreateWithNextToken(solutions, cancelEnumerator, out NextSolutionToken token);

        // Add to concurrent dictionary
        bool requestStarted = _queryResponses.TryAdd(requestId, querier);

        // Return HTTP response with next-solution token
        if (requestStarted)
        {
            return RestResponse.Ok<RestPrologBody.RequestStarted>(new(requestId, token));
        }
        
        try
        {
            await querier.DisposeAsync();
            return RestResponse.BadRequest<RestPrologBody.BadRequest>(new("A request with the given ID is already being processed. If you made the other request with this ID, but lost the corresponding next-solution token, please send a new request."));
        }
        catch (Exception)
        {
            return RestResponse.Problem<RestPrologBody.PrologServerProblemDetails.PrologServerException>(new());
        }
    }

    private async Task<RestResponse> HandleNextSolutionRequestAsync(RestRequest.NextSolutionRequest nextSolution, CancellationToken cancellationToken)
    {
        Guid requestId = nextSolution.RequestId;
        _queryResponses.TryGetValue(requestId, out RestSolutionQuerier? querier);
        if (querier is null)
        {
            return RestResponse.BadRequest<RestPrologBody.BadRequest>(new("Unknown request ID."));
        }

        NextSolutionToken token = nextSolution.NextSolutionToken;
        QuerierResponse querierResponse = await querier.GetNextSolutionAsync(token, cancellationToken);
        RestResponse response = querierResponse switch
        {
            QuerierResponse.NextResponse nextResponse => nextResponse.PrologWorkerResponse switch
            {
                PrologWorkerResponse.ExceptionResponse => RestResponse.Problem<RestPrologBody.PrologServerProblemDetails.PrologServerException>(new()),
                PrologWorkerResponse.EngineResponse engineResponse => engineResponse.Response switch
                {
                    PrologEngineResponse.SolutionResponse solution => RestResponse.Ok<RestPrologBody.RequestSolution>(new(requestId, solution.Value, nextResponse.Token)),
                    PrologEngineResponse.FinalSolutionResponse finalSolution => RestResponse.Ok<RestPrologBody.RequestSolution>(new(requestId, finalSolution.Value, null)),
                    PrologEngineResponse.QueryHaltedResponse => RestResponse.Ok<RestPrologBody.RequestStopped>(new(requestId, RequestStoppedReason.Halted)),
                    PrologEngineResponse.ExceptionResponse => RestResponse.Ok<RestPrologBody.PrologServerProblemDetails.PrologServerException>(new()),
                    _ => RestResponse.NotImplemented<RestPrologBody.PrologServerProblemDetails.PrologNotImplemented>(new())
                },
                _ => RestResponse.NotImplemented<RestPrologBody.PrologServerProblemDetails.PrologNotImplemented>(new())
            },
            QuerierResponse.QueryExhaustedResponse => RestResponse.Ok<RestPrologBody.RequestStopped>(new(requestId, RequestStoppedReason.Exhausted)),
            QuerierResponse.InvalidTokenResponse => RestResponse.BadRequest<RestPrologBody.BadRequest>(new("Invalid next solution token.")),
            QuerierResponse.QuerierInterruptedResponse => RestResponse.BadRequest<RestPrologBody.BadRequest>(new("Query was interrupted.")),
            QuerierResponse.QuerierDisposedResponse => RestResponse.Problem<RestPrologBody.PrologServerProblemDetails.PrologQuerierDisposed>(new()),
            QuerierResponse.PrologWorkerExceptionResponse or
            QuerierResponse.PrologEngineExceptionResponse or
            QuerierResponse.ExceptionResponse => RestResponse.Problem<RestPrologBody.PrologServerProblemDetails.PrologServerException>(new()),
            _ => RestResponse.NotImplemented<RestPrologBody.PrologServerProblemDetails.PrologNotImplemented>(new())
        };
        return response;
    }

    private async Task<RestResponse> HandleStopQueryRequestAsync(RestRequest.StopQueryRequest stopQuery, CancellationToken cancellationToken)
    {
        Guid requestId = stopQuery.RequestId;
        _queryResponses.TryGetValue(requestId, out RestSolutionQuerier? querier);
        if (querier is null)
        {
            return RestResponse.BadRequest<RestPrologBody.BadRequest>(new("Unknown request ID."));
        }

        try
        {
            await querier.DisposeAsync();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Ignore
        }
        catch (Exception)
        {
            return RestResponse.Problem<RestPrologBody.PrologServerProblemDetails.PrologServerException>(new());
        }

        return RestResponse.Ok<RestPrologBody.RequestStopped>(new(requestId, RequestStoppedReason.Halted));
    }
}
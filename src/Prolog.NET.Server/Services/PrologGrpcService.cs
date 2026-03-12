using Grpc.Core;
using Prolog.NET.Actors;
using Prolog.NET.Server.Workers;

namespace Prolog.NET.Server.Services;

/// <summary>
/// gRPC service implementation for the public Prolog query API.
/// </summary>
public sealed class PrologGrpcService(WorkerRegistry registry) : PrologService.PrologServiceBase
{
    /// <summary>
    /// Opens a streaming query and writes solutions to the response stream one by one.
    /// The stream ends with a <see cref="NoMoreSolutions"/> sentinel or an error message.
    /// Client disconnect (<see cref="ServerCallContext.CancellationToken"/>) triggers a
    /// <see cref="WorkerRegistry.CloseQueryAsync"/> to release the worker slot.
    /// </summary>
    public override async Task Query(
        QueryRequest request,
        IServerStreamWriter<SolutionResponse> responseStream,
        ServerCallContext context)
    {
        (string? queryId, string? error) = await registry.OpenQueryAsync(
            request.FilePath, request.Goal, context.CancellationToken);

        if (error != null)
        {
            await responseStream.WriteAsync(new SolutionResponse { Error = error });
            return;
        }

        try
        {
            while (!context.CancellationToken.IsCancellationRequested)
            {
                NextSolutionResponse? next = await registry.NextSolutionAsync(
                    queryId!, context.CancellationToken);

                if (next == null)
                {
                    break;
                }

                switch (next.ResultCase)
                {
                    case NextSolutionResponse.ResultOneofCase.Solution:
                    {
                        SolutionVars sv = new();
                        sv.Variables.Add(next.Solution.Variables);
                        await responseStream.WriteAsync(new SolutionResponse { Solution = sv });
                        break;
                    }

                    case NextSolutionResponse.ResultOneofCase.FinalSolution:
                    {
                        SolutionVars sv = new();
                        sv.Variables.Add(next.FinalSolution.Variables);
                        await responseStream.WriteAsync(new SolutionResponse { Solution = sv });
                        await responseStream.WriteAsync(new SolutionResponse { NoMore = new NoMoreSolutions() });
                        queryId = null;
                        return;
                    }

                    case NextSolutionResponse.ResultOneofCase.NoMore:
                        await responseStream.WriteAsync(new SolutionResponse { NoMore = new NoMoreSolutions() });
                        queryId = null;
                        return;

                    case NextSolutionResponse.ResultOneofCase.Failed:
                        await responseStream.WriteAsync(new SolutionResponse { Error = next.Failed.Error });
                        queryId = null;
                        return;
                }
            }
        }
        finally
        {
            if (queryId != null)
            {
                await registry.CloseQueryAsync(queryId);
            }
        }
    }
}

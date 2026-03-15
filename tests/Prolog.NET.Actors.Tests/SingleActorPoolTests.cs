using Proto;
using Prolog.NET.Actors;

namespace Prolog.NET.Actors.Tests;

[Collection("PrologActors")]
public sealed class SingleActorPoolTests(PrologActorsFixture fixture)
{
    private Task<T> SendAsync<T>(object msg, CancellationToken ct = default) where T : class
        => fixture.ActorSystem.Root.RequestAsync<T>(fixture.SingleWorkerPid, msg, ct);

    [Fact]
    public async Task LoadFile_Succeeds()
    {
        using CancellationTokenSource cts = new(TimeSpan.FromSeconds(15));
        CallResult result = await SendAsync<CallResult>(
            new LoadFileMessage { Path = fixture.PrologFilePath }, cts.Token);

        Assert.True(result.Success);
    }

    [Fact]
    public async Task Call_KnownFact_Succeeds()
    {
        using CancellationTokenSource cts = new(TimeSpan.FromSeconds(15));
        await SendAsync<CallResult>(new LoadFileMessage { Path = fixture.PrologFilePath }, cts.Token);

        using CancellationTokenSource cts2 = new(TimeSpan.FromSeconds(15));
        CallResult result = await SendAsync<CallResult>(
            new CallMessage { Goal = "parent(tom, bob)" }, cts2.Token);

        Assert.True(result.Success);
    }

    [Fact]
    public async Task StreamingQuery_DrainAllSolutions()
    {
        using CancellationTokenSource loadCts = new(TimeSpan.FromSeconds(15));
        await SendAsync<CallResult>(new LoadFileMessage { Path = fixture.PrologFilePath }, loadCts.Token);

        using CancellationTokenSource openCts = new(TimeSpan.FromSeconds(15));
        OpenQueryResponse openResp = await SendAsync<OpenQueryResponse>(
            new OpenQueryMessage { Goal = "ancestor(tom, X)" }, openCts.Token);

        Assert.Equal(OpenQueryResponse.ResultOneofCase.Opened, openResp.ResultCase);
        string queryId = openResp.Opened.QueryId;

        List<string> solutions = [];
        bool done = false;

        while (!done)
        {
            using CancellationTokenSource nextCts = new(TimeSpan.FromSeconds(15));
            NextSolutionResponse resp = await SendAsync<NextSolutionResponse>(
                new NextSolutionMessage { QueryId = queryId }, nextCts.Token);

            switch (resp.ResultCase)
            {
                case NextSolutionResponse.ResultOneofCase.Solution:
                    solutions.Add(resp.Solution.Variables["X"]);
                    break;
                case NextSolutionResponse.ResultOneofCase.FinalSolution:
                    solutions.Add(resp.FinalSolution.Variables["X"]);
                    done = true;
                    break;
                case NextSolutionResponse.ResultOneofCase.NoMore:
                case NextSolutionResponse.ResultOneofCase.Failed:
                    done = true;
                    break;
            }
        }

        Assert.Equal(4, solutions.Count);
        Assert.Contains("bob", solutions);
        Assert.Contains("liz", solutions);
        Assert.Contains("ann", solutions);
        Assert.Contains("pat", solutions);
    }

    [Fact]
    public async Task StreamingQuery_CloseEarly_ThenNewQuerySucceeds()
    {
        using CancellationTokenSource loadCts = new(TimeSpan.FromSeconds(15));
        await SendAsync<CallResult>(new LoadFileMessage { Path = fixture.PrologFilePath }, loadCts.Token);

        using CancellationTokenSource openCts = new(TimeSpan.FromSeconds(15));
        OpenQueryResponse openResp = await SendAsync<OpenQueryResponse>(
            new OpenQueryMessage { Goal = "ancestor(tom, X)" }, openCts.Token);

        Assert.Equal(OpenQueryResponse.ResultOneofCase.Opened, openResp.ResultCase);
        string queryId = openResp.Opened.QueryId;

        // Get one solution
        using CancellationTokenSource nextCts = new(TimeSpan.FromSeconds(15));
        await SendAsync<NextSolutionResponse>(new NextSolutionMessage { QueryId = queryId }, nextCts.Token);

        // Close early via Send (no response expected)
        fixture.ActorSystem.Root.Send(fixture.SingleWorkerPid, new CloseQueryMessage { QueryId = queryId });
        await Task.Delay(300);

        // Open a new query — new PrologQueryActor should be spawned successfully
        using CancellationTokenSource open2Cts = new(TimeSpan.FromSeconds(15));
        OpenQueryResponse open2Resp = await SendAsync<OpenQueryResponse>(
            new OpenQueryMessage { Goal = "ancestor(tom, X)" }, open2Cts.Token);

        Assert.Equal(OpenQueryResponse.ResultOneofCase.Opened, open2Resp.ResultCase);

        // Cleanup
        fixture.ActorSystem.Root.Send(fixture.SingleWorkerPid, new CloseQueryMessage { QueryId = open2Resp.Opened.QueryId });
        await Task.Delay(100);
    }

    [Fact]
    public async Task ConcurrentOpenQueries_SecondFails_WhenPoolExhausted()
    {
        using CancellationTokenSource loadCts = new(TimeSpan.FromSeconds(15));
        await SendAsync<CallResult>(new LoadFileMessage { Path = fixture.PrologFilePath }, loadCts.Token);

        using CancellationTokenSource open1Cts = new(TimeSpan.FromSeconds(15));
        OpenQueryResponse open1Resp = await SendAsync<OpenQueryResponse>(
            new OpenQueryMessage { Goal = "ancestor(tom, X)" }, open1Cts.Token);

        Assert.Equal(OpenQueryResponse.ResultOneofCase.Opened, open1Resp.ResultCase);
        string queryId = open1Resp.Opened.QueryId;

        try
        {
            using CancellationTokenSource open2Cts = new(TimeSpan.FromSeconds(15));
            OpenQueryResponse open2Resp = await SendAsync<OpenQueryResponse>(
                new OpenQueryMessage { Goal = "ancestor(tom, X)" }, open2Cts.Token);

            Assert.Equal(OpenQueryResponse.ResultOneofCase.Failed, open2Resp.ResultCase);
            Assert.Contains("No capacity", open2Resp.Failed.Error);
        }
        finally
        {
            fixture.ActorSystem.Root.Send(fixture.SingleWorkerPid, new CloseQueryMessage { QueryId = queryId });
            await Task.Delay(200);
        }
    }
}

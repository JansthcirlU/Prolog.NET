using Proto;
using Prolog.NET.Actors;

namespace Prolog.NET.Actors.Tests;

[Collection("PrologActors")]
public sealed class MultiActorPoolTests(PrologActorsFixture fixture)
{
    private Task<T> SendAsync<T>(object msg, CancellationToken ct = default) where T : class
        => fixture.ActorSystem.Root.RequestAsync<T>(fixture.MultiWorkerPid, msg, ct);

    [Fact]
    public async Task ConcurrentOpenQueries_BothSucceed_WithPoolSize2()
    {
        using CancellationTokenSource loadCts = new(TimeSpan.FromSeconds(15));
        await SendAsync<CallResult>(new LoadFileMessage { Path = fixture.PrologFilePath }, loadCts.Token);

        using CancellationTokenSource openCts = new(TimeSpan.FromSeconds(15));
        Task<OpenQueryResponse> open1Task = SendAsync<OpenQueryResponse>(
            new OpenQueryMessage { Goal = "ancestor(tom, X)" }, openCts.Token);
        Task<OpenQueryResponse> open2Task = SendAsync<OpenQueryResponse>(
            new OpenQueryMessage { Goal = "ancestor(tom, X)" }, openCts.Token);

        OpenQueryResponse[] responses = await Task.WhenAll(open1Task, open2Task);

        Assert.Equal(OpenQueryResponse.ResultOneofCase.Opened, responses[0].ResultCase);
        Assert.Equal(OpenQueryResponse.ResultOneofCase.Opened, responses[1].ResultCase);

        // Cleanup both queries
        fixture.ActorSystem.Root.Send(fixture.MultiWorkerPid, new CloseQueryMessage { QueryId = responses[0].Opened.QueryId });
        fixture.ActorSystem.Root.Send(fixture.MultiWorkerPid, new CloseQueryMessage { QueryId = responses[1].Opened.QueryId });
        await Task.Delay(300);
    }
}

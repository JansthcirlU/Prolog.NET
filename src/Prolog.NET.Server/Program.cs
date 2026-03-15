using System.Text.Json;
using Prolog.NET.Actors;
using Prolog.NET.Server;
using Prolog.NET.Server.Services;
using Prolog.NET.Server.Workers;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Proto.Remote (client role — routes messages to worker processes).
// Binds to port 4000 so workers can send replies back.
builder.Services.AddPrologServerActors(port: 4000);

// Proto.Remote lifecycle.
builder.Services.AddHostedService<ProtoRemoteService>();

// Worker registry — singleton + hosted service for graceful shutdown.
builder.Services.AddSingleton<WorkerRegistry>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<WorkerRegistry>());

// gRPC.
builder.Services.AddGrpc();

WebApplication app = builder.Build();

// --- gRPC endpoint ---
app.MapGrpcService<PrologGrpcService>();

// --- REST/SSE endpoint ---
// POST /api/query  body: { "filePath": "...", "goal": "..." }
// Response: text/event-stream — each event is a JSON object.
app.MapPost("/api/query", async (QueryRequestDto dto, WorkerRegistry registry, HttpContext ctx) =>
{
    ctx.Response.Headers.ContentType = "text/event-stream";
    ctx.Response.Headers.CacheControl = "no-cache";
    ctx.Response.Headers.Connection = "keep-alive";

    (string? queryId, string? openError) = await registry.OpenQueryAsync(
        dto.FilePath, dto.Goal, ctx.RequestAborted);

    if (openError != null)
    {
        await WriteSseEventAsync(ctx.Response, new { error = openError });
        return;
    }

    try
    {
        while (!ctx.RequestAborted.IsCancellationRequested)
        {
            NextSolutionResponse? next = await registry.NextSolutionAsync(queryId!, ctx.RequestAborted);

            if (next == null)
            {
                break;
            }

            switch (next.ResultCase)
            {
                case NextSolutionResponse.ResultOneofCase.Solution:
                    await WriteSseEventAsync(ctx.Response,
                        new { variables = (object)next.Solution.Variables });
                    break;

                case NextSolutionResponse.ResultOneofCase.FinalSolution:
                    await WriteSseEventAsync(ctx.Response,
                        new { variables = (object)next.FinalSolution.Variables, final = true });
                    await WriteSseEventAsync(ctx.Response, new { noMore = true });
                    queryId = null;
                    return;

                case NextSolutionResponse.ResultOneofCase.NoMore:
                    await WriteSseEventAsync(ctx.Response, new { noMore = true });
                    queryId = null;
                    return;

                case NextSolutionResponse.ResultOneofCase.Failed:
                    await WriteSseEventAsync(ctx.Response, new { error = next.Failed.Error });
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
});

// Health check.
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

await app.RunAsync();

static async Task WriteSseEventAsync(HttpResponse response, object payload)
{
    string json = JsonSerializer.Serialize(payload);
    await response.WriteAsync($"data: {json}\n\n");
    await response.Body.FlushAsync();
}

/// <summary>Request body for the REST/SSE query endpoint.</summary>
internal sealed record QueryRequestDto(string FilePath, string Goal);

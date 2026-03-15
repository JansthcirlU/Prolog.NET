using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Prolog.NET.Actors;
using Prolog.NET.Swipl;
using Prolog.NET.Worker;

// Parse --port <n> from args before passing anything to the host builder.
string? portString = args.SkipWhile(a => a != "--port").Skip(1).FirstOrDefault();
if (!int.TryParse(portString, out int port))
{
    await Console.Error.WriteLineAsync("Usage: Prolog.NET.Worker --port <n>");
    return;
}

HostApplicationBuilder builder = Host.CreateApplicationBuilder();

builder.Services
    .AddPrologEngine()
    .AddPrologActors(port)
    .AddHostedService<PrologWorker>();

await builder.Build().RunAsync();

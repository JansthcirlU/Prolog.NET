using Microsoft.Extensions.Hosting;
using Prolog.NET.Swipl;
using Prolog.NET.Actors;
using Prolog.NET.Console;
using Microsoft.Extensions.DependencyInjection;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddPrologEngine()
    .AddPrologActors()
    .AddHostedService<PrologWorker>();

await builder.Build().RunAsync();

using Microsoft.Extensions.Hosting;
using Prolog.NET.Actors;
using Prolog.NET.Console;
using Microsoft.Extensions.DependencyInjection;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddCliActor()
    .AddHostedService<PrologServer>();

await builder.Build().RunAsync();

using CommandLine;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QRWells.MapReduce;
using QRWells.MapReduce.Extensions;
using QRWells.MapReduce.Method;
using QRWells.MapReduce.Rpc.Extensions;

var runtimeArgs = Parser.Default
    .ParseArguments<CoordinatorOptions, WorkerOptions>(args)
    .MapResult(
        (CoordinatorOptions config) => new RuntimeArgs
        {
            Port = config.Port,
            IsCoordinator = true,
            Timeout = (uint)config.Timeout,
            NumberReduce = (uint)config.NumberReduce,
            Files = config.Files
        },
        (WorkerOptions config) => new RuntimeArgs
        {
            CoordinatorHost = config.Host,
            Port = config.Port,
            IsCoordinator = false,
            Task = config.Task,
            Timeout = (uint)config.Timeout
        },
        _ => new RuntimeArgs()
    )!;

MethodProxy? methodProxy = null;
if (!runtimeArgs.IsCoordinator)
{
    if (string.IsNullOrEmpty(runtimeArgs.Task)) return;

    var file = new FileInfo(runtimeArgs.Task);
    if (!file.Exists)
    {
        Console.WriteLine($"File {runtimeArgs.Task} not found.");
        return;
    }

    if (!MethodLoader.TryLoad(file.FullName, out methodProxy))
    {
        Console.WriteLine("Failed to load method.");
        return;
    }
}

if (runtimeArgs.IsCoordinator)
{
    var builder = WebApplication.CreateBuilder(args);
    builder.WebHost.UseUrls($"http://localhost:{runtimeArgs.Port}");
    builder.Services.AddLogging();
    builder.Services.UseCoordinator(runtimeArgs.NumberReduce, runtimeArgs.Files);
    var app = builder.Build();
    app.MapRpc();
    app.Run();
}
else
{
    var builder = new HostBuilder();
    builder.ConfigureServices((hostContext, services) =>
    {
        services.AddLogging(s => s.AddDebug().AddConsole());
        services.UseWorker(worker =>
        {
            worker.Host = runtimeArgs.CoordinatorHost!;
            worker.Port = runtimeArgs.Port;
            worker.MethodProxy = methodProxy!;
        });
    });
    await builder.RunConsoleAsync();
}
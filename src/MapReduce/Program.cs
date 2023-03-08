using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QRWells.MapReduce;
using QRWells.MapReduce.Extensions;
using QRWells.MapReduce.Method;

var runtimeArgs = Parser.Default.ParseArguments<CoordinatorOptions, WorkerOptions>(args).MapResult( // (1)
    (CoordinatorOptions config) => new RuntimeArgs
    {
        Port = config.Port,
        IsCoordinator = true,
        Timeout = config.Timeout,
        NumberReduce = config.NumberReduce,
        Files = config.Files
    },
    (WorkerOptions config) => new RuntimeArgs
    {
        CoordinatorHost = config.Host,
        Port = config.Port,
        IsCoordinator = false,
        Task = config.Task,
        Timeout = config.Timeout
    },
    _ => new RuntimeArgs()
)!;

MethodProxy? methodProxy = null;
if (!runtimeArgs.IsCoordinator)
    if (!MethodLoader.TryLoad(runtimeArgs.Task, out methodProxy))
    {
        Console.WriteLine("Failed to load method.");
        return;
    }

var builder = new HostBuilder();
builder.ConfigureServices((hostContext, services) =>
{
    services.AddLogging();
    if (runtimeArgs.IsCoordinator)
        services.UseCoordinator(runtimeArgs.NumberReduce, runtimeArgs.Port, runtimeArgs.Files,
            c => { c.SetTimeout(runtimeArgs.Timeout); });
    else
        services.UseWorker(worker =>
        {
            worker.Host = runtimeArgs.CoordinatorHost!;
            worker.Port = runtimeArgs.Port;
            worker.MethodProxy = methodProxy!;
        });
});

await builder.RunConsoleAsync();
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using QRWells.MapReduce.Hosts;
using QRWells.MapReduce.Rpc.Extensions;

namespace QRWells.MapReduce.Extensions;

public static class MapReduceExtension
{
    public static IServiceCollection UseCoordinator(this IServiceCollection services, uint nReduce,
        IEnumerable<string> files, Action<Coordinator>? configure = null)
    {
        services.AddSingleton(new CoordinatorConfig
        {
            Files = files,
            NumberReduce = nReduce,
            Configure = configure
        });
        return services.ConfigureRpc(service => { service.Assembly = Assembly.GetEntryAssembly()!; });
    }

    public static IServiceCollection UseWorker(this IServiceCollection services,
        Action<Worker>? configure = null)
    {
        services.AddSingleton(new WorkerConfig
        {
            Configure = configure
        });
        return services.AddHostedService<WorkerHost>();
    }
}
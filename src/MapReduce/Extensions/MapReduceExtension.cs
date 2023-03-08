using Microsoft.Extensions.DependencyInjection;
using QRWells.MapReduce.Hosts;
using QRWells.MapReduce.Rpc.Extensions;
using QRWells.MapReduce.Rpc.Server;

namespace QRWells.MapReduce.Extensions;

public static class MapReduceExtension
{
    public static IServiceCollection UseCoordinator(this IServiceCollection services, uint nReduce, int port,
        IEnumerable<string> files,
        Action<Coordinator>? configure, Action<RpcServer>? configureRpc = null)
    {
        services.AddSingleton(new CoordinatorConfig
        {
            Files = files,
            NumberReduce = nReduce,
            Configure = configure
        });
        return services.ConfigureRpc();
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
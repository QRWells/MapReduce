using Microsoft.Extensions.DependencyInjection;
using QRWells.MapReduce.Hosts;

namespace QRWells.MapReduce.Extensions;

public static class MapReduceExtension
{
    public static IServiceCollection UseCoordinator(this IServiceCollection services,
        Action<Coordinator> configure = null)
    {
        services.AddSingleton(new CoordinatorConfig
        {
            Configure = configure
        });
        return services.AddHostedService<CoordinatorHost>();
    }

    public static IServiceCollection UseWorker(this IServiceCollection services,
        Action<Worker> configure = null)
    {
        services.AddSingleton(new WorkerConfig
        {
            Configure = configure
        });
        return services.AddHostedService<WorkerHost>();
    }
}
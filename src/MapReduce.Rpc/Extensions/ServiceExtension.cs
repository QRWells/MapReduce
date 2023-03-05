using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using QRWells.MapReduce.Rpc.Server;

namespace QRWells.MapReduce.Rpc.Extensions;

public static class ServiceExtension
{
    public static IServiceCollection UseRpc(this IServiceCollection services, Assembly assembly, int port = 8080,
        Action<RpcServer>? serverConfig = null)
    {
        services.AddSingleton(new RpcServerArgs
        {
            Assembly = assembly,
            ServerConfiguration = serverConfig,
            Port = port,
            Services = services
        });
        return services.AddHostedService<RpcServerHost>();
    }
}
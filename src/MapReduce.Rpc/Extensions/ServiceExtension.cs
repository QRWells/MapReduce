using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using QRWells.MapReduce.Rpc.Server;

namespace QRWells.MapReduce.Rpc.Extensions;

public static class ServiceExtension
{
    public static IServiceCollection UseRpc(this IServiceCollection services, Assembly assembly)
    {
        services.AddSingleton(new RpcServerArgs
        {
            Assembly = assembly
        });
        return services.AddHostedService<RpcServerHost>();
    }
}
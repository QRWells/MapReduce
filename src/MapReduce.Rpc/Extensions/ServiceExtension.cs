using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using QRWells.MapReduce.Rpc.Codecs;
using QRWells.MapReduce.Rpc.Server;
using QRWells.MapReduce.Rpc.Service;

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
        if (services.All(x => x.ServiceType != typeof(ICodec))) services.AddSingleton<ICodec, JsonCodec>();

        return services.AddHostedService<RpcServerHost>();
    }

    public static IServiceCollection ConfigureRpc(this IServiceCollection services, Action<RpcService>? config)
    {
        var service = new RpcService();
        config?.Invoke(service);
        service.Init(services);
        return services.AddSingleton(service);
    }

    public static IServiceCollection ConfigureRpc(this IServiceCollection services)
    {
        var service = new RpcService();
        service.Init(services);
        return services.AddSingleton(service);
    }

    public static IEndpointConventionBuilder MapRpc(this IEndpointRouteBuilder endpoints, string basePath = "/rpc")
    {
        return endpoints.MapPost(basePath, async (HttpRequest request, RpcService service) =>
        {
            var rpcResponse = await service.Invoke(request.Body);
            return rpcResponse.Error is not null
                ? Results.BadRequest(rpcResponse.Error)
                : Results.Ok(rpcResponse.Result);
        });
    }
}
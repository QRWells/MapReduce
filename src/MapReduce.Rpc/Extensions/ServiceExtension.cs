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
        return endpoints.MapPost(basePath, async (HttpContext context, RpcService service) =>
        {
            var rpcResponse = await service.Invoke(context.Request.Body);
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = rpcResponse.Error != null ? 400 : 200;
            await context.Response.WriteAsync(rpcResponse.Result!);
        });
    }
}
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QRWells.MapReduce.Rpc.Attributes;

namespace QRWells.MapReduce.Rpc.Server;

public class RpcServerHost : IHostedService
{
    private readonly RpcServerArgs _rpcServerArgs;
    private readonly IServiceCollection _rpcServices = new ServiceCollection();
    private RpcServer _rpcServer;
    private IServiceProvider _rpcServiceProvider;

    public RpcServerHost(RpcServerArgs args)
    {
        _rpcServerArgs = args;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _rpcServer = new RpcServer();
        foreach (var type in _rpcServerArgs.Assembly.GetTypes())
        {
            var service = type.GetCustomAttribute<ServiceAttribute>(false);
            if (service == null) continue;
            var serviceDescriptor = new ServiceDescriptor(service.Contract, type, service.Lifetime);
            _rpcServices.Add(serviceDescriptor);
        }

        _rpcServiceProvider = _rpcServices.BuildServiceProvider();

        _rpcServer.Start();

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _rpcServices.Clear();
        _rpcServer.Dispose();
        return Task.CompletedTask;
    }
}
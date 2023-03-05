using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QRWells.MapReduce.Rpc.Attributes;

namespace QRWells.MapReduce.Rpc.Server;

public class RpcServerHost : IHostedService
{
    private readonly ILogger<RpcServerHost> _logger;
    private readonly RpcServerArgs _rpcServerArgs;
    private readonly IServiceCollection _rpcServices = new ServiceCollection();
    private RpcServer _rpcServer;
    private IServiceProvider _rpcServiceProvider;

    public RpcServerHost(RpcServerArgs args, ILogger<RpcServerHost> logger)
    {
        _rpcServerArgs = args;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        {
            var array = new ServiceDescriptor[_rpcServerArgs.Services.Count];
            _rpcServerArgs.Services.CopyTo(array, 0);
            _rpcServices.Add(array);
        }

        foreach (var type in _rpcServerArgs.Assembly.GetTypes())
        {
            var service = type.GetCustomAttribute<ServiceAttribute>(false);
            if (service == null) continue;
            var serviceDescriptor = new ServiceDescriptor(service.Contract, type, service.Lifetime);
            _rpcServices.Add(serviceDescriptor);
        }

        _rpcServiceProvider = _rpcServices.BuildServiceProvider();
        _rpcServer = new RpcServer(_rpcServerArgs.Port, _rpcServiceProvider);
        _rpcServerArgs.ServerConfiguration?.Invoke(_rpcServer);
        _rpcServer.Open();

        _logger.LogInformation("RPC Server started on port {Port}", _rpcServerArgs.Port);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _rpcServices.Clear();
        _rpcServer.Dispose();
        _logger.LogInformation("RPC Server stopped");
        return Task.CompletedTask;
    }
}
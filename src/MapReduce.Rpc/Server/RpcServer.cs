using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetCoreServer;

namespace QRWells.MapReduce.Rpc.Server;

public class RpcServer : HttpServer
{
    internal readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RpcServer> _logger;

    internal RpcServer(int port, IServiceProvider serviceProvider) : base(IPAddress.Any, port)
    {
        _serviceProvider = serviceProvider;
        _logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<RpcServer>();
    }

    protected override TcpSession CreateSession()
    {
        return new RpcSession(this);
    }

    public void Open()
    {
        Start();
    }
}
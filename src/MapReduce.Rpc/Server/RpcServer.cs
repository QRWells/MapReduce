using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace QRWells.MapReduce.Rpc.Server;

public class RpcServer
{
    private readonly ILogger<RpcServer> _logger;
    internal readonly IServiceProvider _serviceProvider;

    internal RpcServer(int port, IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<RpcServer>();
    }
}
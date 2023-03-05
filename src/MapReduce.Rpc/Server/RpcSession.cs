using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetCoreServer;

namespace QRWells.MapReduce.Rpc.Server;

public class RpcSession : HttpSession
{
    private readonly ILogger<RpcSession> _logger;
    private readonly IServiceProvider _serviceProvider;
    // a temporary buffer for data to be decoded
    private readonly byte[] _buffer = new byte[1024];
    public RpcSession(RpcServer server) : base(server)
    {
        _serviceProvider = server._serviceProvider;
        _logger = _serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<RpcSession>();
    }

    protected override void OnReceivedRequest(HttpRequest request)
    {
        
    }
}
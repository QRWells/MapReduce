using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace QRWells.MapReduce.Rpc.Server;

public class RpcServerArgs
{
    public Assembly Assembly { get; init; }
    public Action<RpcServer>? ServerConfiguration { get; init; }
    public IServiceCollection Services { get; init; }
    public int Port { get; init; } = 8080;
}
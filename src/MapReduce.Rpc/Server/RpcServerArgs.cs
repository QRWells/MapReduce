using System.Reflection;

namespace QRWells.MapReduce.Rpc.Server;

public class RpcServerArgs
{
    public Assembly Assembly { get; init; }
    public int Port { get; init; } = 8080;
}
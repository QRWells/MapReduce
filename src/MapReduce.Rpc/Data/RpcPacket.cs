using System.Reflection;

namespace QRWells.MapReduce.Rpc.Data;

public class RpcPacket : IDisposable
{
    public Type InterfaceType { get; set; }
    public MethodInfo Method { get; set; }
    public object[] Arguments { get; set; }

    public void Dispose()
    {
    }
}
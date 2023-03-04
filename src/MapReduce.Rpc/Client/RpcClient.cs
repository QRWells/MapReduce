using System.Reflection;

namespace QRWells.MapReduce.Rpc.Client;

public class RpcClient
{
    public RpcClient(string host, int port)
    {
    }

    public T GetService<T>() where T : class
    {
        var proxy = DispatchProxy.Create<T, CallingProxy>();
        var callingProxy = (CallingProxy)(object)proxy;
        callingProxy.InterfaceType = typeof(T);
        callingProxy.Client = this;
        return proxy;
    }
}
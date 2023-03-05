using System.Reflection;
using QRWells.MapReduce.Rpc.Data;
using HttpClient = NetCoreServer.HttpClient;

namespace QRWells.MapReduce.Rpc.Client;

public class RpcClient : HttpClient
{
    private readonly Dictionary<Type, CallingProxy> _proxyCache = new();

    public RpcClient(string host, int port) : base(host, port)
    {
    }

    public T GetService<T>() where T : class
    {
        if (_proxyCache.TryGetValue(typeof(T), out var p)) return (T)(object)p;

        var proxy = DispatchProxy.Create<T, CallingProxy>();
        var callingProxy = (CallingProxy)(object)proxy;
        callingProxy.InterfaceType = typeof(T);
        callingProxy.Client = this;
        callingProxy.Build();
        _proxyCache.Add(typeof(T), callingProxy);
        return proxy;
    }

    public Task<RpcPacket> Call(RpcPacket packet)
    {
        return Task.FromResult(packet);
    }
}
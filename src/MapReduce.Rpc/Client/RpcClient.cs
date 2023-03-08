using System.Reflection;
using QRWells.MapReduce.Rpc.Codecs;

namespace QRWells.MapReduce.Rpc.Client;

public class RpcClient : HttpClient
{
    private readonly Dictionary<Type, CallingProxy> _proxyCache = new();

    public RpcClient(string host, int port = 80, string path = "/rpc")
    {
        ReadOnlySpan<char> RemoveSlash(ReadOnlySpan<char> span)
        {
            if (span[0] == '/') span = span[1..];
            if (span[^1] == '/') span = span[..^1];
            return span;
        }

        BaseAddress = new Uri($"http://{RemoveSlash(host)}:{port}/{RemoveSlash(path)}");
    }

    public ICodec Codec { get; set; } = new JsonCodec();

    public T GetService<T>(string? name = null) where T : class
    {
        var type = typeof(T);
        if (_proxyCache.TryGetValue(type, out var p)) return (T)(object)p;

        var proxy = DispatchProxy.Create<T, CallingProxy>();
        var callingProxy = (CallingProxy)(object)proxy;
        callingProxy.ServiceName = name ?? type.Name;
        callingProxy.InterfaceType = type;
        callingProxy.Client = this;
        _proxyCache.Add(type, callingProxy);
        return proxy;
    }
}
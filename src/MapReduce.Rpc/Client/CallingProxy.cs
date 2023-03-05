using System.Reflection;
using QRWells.MapReduce.Rpc.Attributes;

namespace QRWells.MapReduce.Rpc.Client;

public class CallingProxy : DispatchProxy
{
    private readonly Dictionary<string, RpcCallHandler> _handlers = new();
    public Type InterfaceType { get; set; }
    public RpcClient Client { get; set; }

    internal void Build()
    {
        var type = InterfaceType;
        foreach (var method in type.GetMethods(BindingFlags.Instance | BindingFlags.Public))
        {
            if (method.GetCustomAttribute<IgnoreAttribute>() != null) continue;
            var handler = new RpcCallHandler(method);
            _handlers.Add(method.Name, handler);
        }
    }

    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
    {
        throw new NotImplementedException();
    }
}
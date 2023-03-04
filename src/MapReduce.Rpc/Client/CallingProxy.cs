using System.Reflection;

namespace QRWells.MapReduce.Rpc.Client;

public class CallingProxy : DispatchProxy
{
    public Type InterfaceType { get; set; }
    public RpcClient Client { get; set; }

    internal void Build()
    {
        var type = InterfaceType;
        foreach (var method in type.GetMethods(BindingFlags.Instance | BindingFlags.Public))
        {
            // todo implement
        }
    }

    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
    {
        throw new NotImplementedException();
    }
}
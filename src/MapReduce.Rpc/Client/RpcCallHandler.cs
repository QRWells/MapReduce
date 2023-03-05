using System.Reflection;

namespace QRWells.MapReduce.Rpc.Client;

public class RpcCallHandler
{
    public RpcCallHandler(MethodInfo methodInfo)
    {
        MethodInfo = methodInfo;
        ResultType = methodInfo.ReturnType;
    }

    public Type ResultType { get; set; }
    public MethodInfo MethodInfo { get; set; }
}
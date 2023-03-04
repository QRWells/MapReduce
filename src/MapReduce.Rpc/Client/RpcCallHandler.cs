using System.Runtime.Serialization;

namespace QRWells.MapReduce.Rpc.Client;

public class RpcCallHandler : ISerializable
{
    public string MethodName { get; set; }
    public Dictionary<string, object> Parameters { get; set; }

    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
    }
}
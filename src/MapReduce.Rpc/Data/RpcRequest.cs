namespace QRWells.MapReduce.Rpc.Data;

[Serializable]
public class RpcRequest
{
    public string Service { get; set; }
    public string Method { get; set; }
    public Dictionary<string, object?> Parameters { get; set; } = new();
}
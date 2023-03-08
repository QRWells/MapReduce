namespace QRWells.MapReduce.Rpc.Data;

public class RpcResponse
{
    public object? Result { get; set; }
    public string? Error { get; set; }

    public T GetResult<T>()
    {
        return (T)Result!;
    }

    public bool HasError()
    {
        return Error != null;
    }
}
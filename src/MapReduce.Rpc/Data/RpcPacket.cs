using System.IO.Pipelines;

namespace QRWells.MapReduce.Rpc.Data;

public class RpcPacket : IDisposable
{
    public bool Read(PipeReader reader)
    {
        return true;
    }

    public void Write(PipeWriter writer)
    {
    }

    public void Dispose()
    {
    }
}
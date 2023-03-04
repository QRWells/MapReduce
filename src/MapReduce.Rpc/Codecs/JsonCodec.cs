using QRWells.MapReduce.Rpc.Client;

namespace QRWells.MapReduce.Rpc.Codecs;

public class JsonCodec : ICodec
{
    public byte[] Encode(RpcCallHandler obj)
    {
        throw new NotImplementedException();
    }

    public RpcCallHandler Decode(byte[] bytes)
    {
        throw new NotImplementedException();
    }
}
using QRWells.MapReduce.Rpc.Client;

namespace QRWells.MapReduce.Rpc.Codecs;

public interface ICodec
{
    byte[] Encode(RpcCallHandler obj);
    RpcCallHandler Decode(byte[] bytes);
}
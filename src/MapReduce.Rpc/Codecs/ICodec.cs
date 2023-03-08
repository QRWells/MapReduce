namespace QRWells.MapReduce.Rpc.Codecs;

public interface ICodec
{
    Task<string> EncodeAsync<T>(T obj);
    Task<T?> DecodeAsync<T>(Stream bytes);
}
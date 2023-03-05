namespace QRWells.MapReduce.Method;

public interface IMapper
{
    IEnumerable<KeyValuePair<string, string>> Map(string key, string value);
}
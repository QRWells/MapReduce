namespace QRWells.MapReduce;

public interface IMapper
{
    IEnumerable<KeyValuePair<string, string>> Map(string key, string value);
}
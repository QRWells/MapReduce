namespace QRWells.MapReduce.Method;

public abstract class MethodProxy : IMapper, IReducer
{
    public abstract IEnumerable<KeyValuePair<string, string>> Map(string key, string value);

    public abstract string Reduce(string key, IEnumerable<string> values);
}
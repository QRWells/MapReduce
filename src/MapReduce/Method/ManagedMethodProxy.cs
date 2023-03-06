namespace QRWells.MapReduce.Method;

public class ManagedMethodProxy : MethodProxy
{
    private readonly IMapper _mapperInstance;
    private readonly IReducer _reducerInstance;

    internal ManagedMethodProxy(IMapper mapperInstance, IReducer reducerInstance)
    {
        _mapperInstance = mapperInstance;
        _reducerInstance = reducerInstance;
    }

    public override IEnumerable<KeyValuePair<string, string>> Map(string key, string value)
    {
        return _mapperInstance.Map(key, value);
    }

    public override string Reduce(string key, IEnumerable<string> values)
    {
        return _reducerInstance.Reduce(key, values);
    }
}
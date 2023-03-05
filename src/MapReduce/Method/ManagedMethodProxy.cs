using System.Reflection;

namespace QRWells.MapReduce.Method;

public class ManagedMethodProxy : MethodProxy
{
    private object callerInstance;
    private MethodInfo mapMethod;
    private MethodInfo reduceMethod;

    public override IEnumerable<KeyValuePair<string, string>> Map(string key, string value)
    {
        return (IEnumerable<KeyValuePair<string, string>>)
            (mapMethod.Invoke(null, new object[] { key, value }) ??
             Enumerable.Empty<KeyValuePair<string, string>>());
    }

    public override string Reduce(string key, IEnumerable<string> values)
    {
        return (string)(reduceMethod.Invoke(null, new object[] { key, values }) ?? string.Empty);
    }
}
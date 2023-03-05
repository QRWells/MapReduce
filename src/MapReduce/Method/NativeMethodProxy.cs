namespace QRWells.MapReduce.Method;

public class NativeMethodProxy : MethodProxy
{
    public override IEnumerable<KeyValuePair<string, string>> Map(string key, string value)
    {
        throw new NotImplementedException();
    }

    public override string Reduce(string key, IEnumerable<string> values)
    {
        throw new NotImplementedException();
    }
}
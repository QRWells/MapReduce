namespace QRWells.MapReduce.Loader;

public interface IMethodLoader : IDisposable
{
    public bool TryLoad(string path);
    public IEnumerable<(string, string)> Map(string key, string value);
    public string Reduce(string key, IEnumerable<string> values);
}
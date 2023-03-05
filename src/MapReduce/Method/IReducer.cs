namespace QRWells.MapReduce.Method;

public interface IReducer
{
    string Reduce(string key, IEnumerable<string> values);
}
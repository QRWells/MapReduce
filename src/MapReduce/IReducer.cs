namespace QRWells.MapReduce;

public interface IReducer
{
    string Reduce(string key, IEnumerable<string> values);
}
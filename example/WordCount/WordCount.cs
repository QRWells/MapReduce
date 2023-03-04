using QRWells.MapReduce;

namespace Example.WordCount;

public class WordCount : IMapper, IReducer
{
    public IEnumerable<KeyValuePair<string, string>> Map(string key, string value)
    {
        return value
            .Split(new[] { ' ' }, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .GroupBy(word => word)
            .Select(group => new KeyValuePair<string, string>(group.Key, group.Count().ToString()));
    }

    public string Reduce(string key, IEnumerable<string> values)
    {
        var sum = values.Sum(int.Parse);
        return sum.ToString();
    }
}
using QRWells.MapReduce.Method;

namespace Example.WordCount;

public class WordCount : IMapper, IReducer
{
    public IEnumerable<KeyValuePair<string, string>> Map(string key, string value)
    {
        return ExtractWords(value)
            .GroupBy(word => word)
            .Select(group => new KeyValuePair<string, string>(group.Key, group.Count().ToString()));
    }

    public string Reduce(string key, IEnumerable<string> values)
    {
        var sum = values.Sum(int.Parse);
        return sum.ToString();
    }

    private static IEnumerable<string> ExtractWords(string value)
    {
        var list = new List<string>();
        var span = value.AsSpan();
        for (var i = 0; i < span.Length; i++)
        {
            var c = span[i];
            if (!char.IsLetter(c)) continue;
            var start = i;
            while (i < span.Length && char.IsLetter(span[i]))
                i++;
            list.Add(span[start..i].ToString());
        }

        return list;
    }
}
using System.Reflection;

namespace QRWells.MapReduce.Loader;

public class AssemblyLoader : IMethodLoader
{
    private MethodInfo? _map;
    private MethodInfo? _reduce;

    public bool TryLoad(string path)
    {
        var assembly = Assembly.LoadFrom(path);
        var types = assembly.GetTypes();
        foreach (var type in types)
        {
            if (!TryFindMethod(type, out var map, out var reduce)) continue;
            _map = map;
            _reduce = reduce;
            return true;
        }

        return false;
    }

    public IEnumerable<(string, string)> Map(string key, string value)
    {
        if (_map == null) throw new InvalidOperationException("Map method not found");
        return (IEnumerable<(string, string)>)_map.Invoke(null, new object[] { key, value })!;
    }

    public string Reduce(string key, IEnumerable<string> values)
    {
        if (_reduce == null) throw new InvalidOperationException("Reduce method not found");
        return (string)_reduce.Invoke(null, new object[] { key, values })!;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    private static bool TryFindMethod(Type type, out MethodInfo? map, out MethodInfo? reduce)
    {
        map = default;
        reduce = default;
        var methods = type.GetMethods();
        foreach (var method in methods)
            switch (method.Name)
            {
                case "Map":
                {
                    var parameters = method.GetParameters();
                    if (parameters.Length != 2 ||
                        parameters[0].ParameterType != typeof(string) ||
                        parameters[1].ParameterType != typeof(string) ||
                        method.ReturnType != typeof(IEnumerable<(string, string)>))
                        return false;

                    map = method;

                    break;
                }

                case "Reduce":
                {
                    var parameters = method.GetParameters();

                    if (parameters.Length != 2 ||
                        parameters[0].ParameterType != typeof(string) ||
                        parameters[1].ParameterType != typeof(IEnumerable<string>) ||
                        method.ReturnType != typeof(string))
                        return false;

                    reduce = method;

                    break;
                }
            }

        return true;
    }
}
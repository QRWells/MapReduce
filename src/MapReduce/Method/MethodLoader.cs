using System.Reflection;
using System.Runtime.InteropServices;

namespace QRWells.MapReduce.Method;

public static class MethodLoader
{
    /// <summary>
    ///     Try to load a method from a library or assembly.
    ///     if the library is a assembly, the class implementing <see cref="IMapper" /> or <see cref="IReducer" />
    ///     or both must be public and have a public parameterless constructor.
    ///     If the library is a native library, the function need to be exported with the following signature:
    ///     <code>
    ///         void map(char* key, char* value, char** result_key, char** result_value, int* count);
    ///         char* reduce(char* key, char** values, int count);
    ///     </code>
    /// </summary>
    /// <param name="path"></param>
    /// <param name="proxy"></param>
    /// <returns></returns>
    public static bool TryLoad(string path, out MethodProxy? proxy)
    {
        proxy = default;
        try
        {
            var assembly = Assembly.LoadFile(path);
            proxy = LoadManaged(assembly);
            return true;
        }
        catch (BadImageFormatException e)
        {
            try
            {
                var libHandle = NativeLibrary.Load(path);
                proxy = LoadNative(libHandle);
                return true;
            }
            catch (Exception)
            {
                proxy = default;
                return false;
            }
        }
        catch (ArgumentException e)
        {
            Console.WriteLine(e);
            return false;
        }
    }

    private static ManagedMethodProxy LoadManaged(Assembly assembly)
    {
        IMapper? mapperInstance = default;
        IReducer? reducerInstance = default;
        foreach (var type in assembly.GetTypes())
        {
            if (type.IsAbstract || type.IsInterface)
                continue;
            if (type.GetInterface(nameof(IMapper)) is not null &&
                type.GetInterface(nameof(IReducer)) is not null) // both mapper and reducer are implemented
            {
                var instance = Activator.CreateInstance(type);
                mapperInstance = instance as IMapper;
                reducerInstance = instance as IReducer;
                break;
            }

            if (type.GetInterface(nameof(IMapper)) is not null && mapperInstance is null)
                mapperInstance = Activator.CreateInstance(type) as IMapper;

            if (type.GetInterface(nameof(IReducer)) is not null && reducerInstance is null)
                reducerInstance = Activator.CreateInstance(type) as IReducer;
        }

        if (mapperInstance is not null && reducerInstance is not null)
            return new ManagedMethodProxy(mapperInstance, reducerInstance);

        throw new Exception("Cannot find valid mapper or reducer.");
    }

    private static NativeMethodProxy LoadNative(nint libHandle)
    {
        var map = NativeLibrary.GetExport(libHandle, "map");
        var reduce = NativeLibrary.GetExport(libHandle, "reduce");
        if (map > 0 && reduce > 0)
            return new NativeMethodProxy(libHandle, map, reduce);

        throw new Exception("Cannot find valid mapper or reducer.");
    }
}
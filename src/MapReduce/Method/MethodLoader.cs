using System.Reflection;
using System.Runtime.InteropServices;

namespace QRWells.MapReduce.Method;

public static class MethodLoader
{
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
    }

    private static ManagedMethodProxy LoadManaged(Assembly assembly)
    {
        throw new NotImplementedException();
    }

    private static NativeMethodProxy LoadNative(nint libHandle)
    {
        throw new NotImplementedException();
    }
}
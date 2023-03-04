using System.Runtime.InteropServices;

namespace QRWells.MapReduce.Loader;

public class NativeLoader : IMethodLoader
{
    private nint _map;
    private nint _nativeHandle;
    private nint _reduce;

    public bool TryLoad(string path)
    {
        if (!NativeLibrary.TryLoad(path, out _nativeHandle)) return false;
        return NativeLibrary.TryGetExport(_nativeHandle, "map", out _map) &&
               NativeLibrary.TryGetExport(_nativeHandle, "reduce", out _reduce);
    }

    public IEnumerable<(string, string)> Map(string key, string value)
    {
        throw new NotImplementedException();
    }

    public string Reduce(string key, IEnumerable<string> values)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        NativeLibrary.Free(_nativeHandle);

        GC.SuppressFinalize(this);
    }

    // todo: use the bittable prototype
    private delegate IEnumerable<(string, string)> MapDelegate(string key, string value);

    private delegate string ReduceDelegate(string key, IEnumerable<string> values);
}
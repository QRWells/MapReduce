using System.Runtime.InteropServices;

namespace QRWells.MapReduce.Method;

public class NativeMethodProxy : MethodProxy, IDisposable
{
    private readonly nint _libHandle;
    private readonly MapDelegate _mapHandle;
    private readonly ReduceDelegate _reduceHandle;

    internal NativeMethodProxy(nint libHandle, nint mapHandle, nint reduceHandle)
    {
        _libHandle = libHandle;
        _mapHandle = Marshal.GetDelegateForFunctionPointer<MapDelegate>(mapHandle);
        _reduceHandle = Marshal.GetDelegateForFunctionPointer<ReduceDelegate>(reduceHandle);
    }

    public void Dispose()
    {
        NativeLibrary.Free(_libHandle);
    }

    public override IEnumerable<KeyValuePair<string, string>> Map(string key, string value)
    {
        var result = new List<KeyValuePair<string, string>>();
        unsafe
        {
            var keyPtr = Marshal.StringToHGlobalAuto(key);
            var valuePtr = Marshal.StringToHGlobalAuto(value);
            var resultKey = (char**)NativeMemory.AlignedAlloc(4096, 4096);
            var resultValue = (char**)NativeMemory.AlignedAlloc(4096, 4096);
            var count = 0;
            _mapHandle.Invoke((char*)keyPtr, (char*)valuePtr, resultKey, resultValue, &count);
            for (var i = 0; i < count; i++)
            {
                var keyStr = Marshal.PtrToStringAuto((nint)resultKey[i]);
                var valueStr = Marshal.PtrToStringAuto((nint)resultValue[i]);
                result.Add(new KeyValuePair<string, string>(keyStr, valueStr));
            }

            NativeMemory.AlignedFree(resultKey);
            NativeMemory.AlignedFree(resultValue);
            Marshal.FreeHGlobal(valuePtr);
            Marshal.FreeHGlobal(keyPtr);
        }

        return result;
    }

    public override string Reduce(string key, IEnumerable<string> values)
    {
        string result;
        var count = values.Count();
        unsafe
        {
            var keyPtr = Marshal.StringToHGlobalAuto(key);
            var valuePtr = Marshal.AllocHGlobal(sizeof(char*) * count);
            var i = 0;
            foreach (var value in values)
            {
                var valuePtrIt = Marshal.StringToHGlobalAuto(value);
                Marshal.WriteIntPtr(valuePtr, i * sizeof(char*), valuePtrIt);
                i++;
            }

            var resultPtr = _reduceHandle.Invoke((char*)keyPtr, (char**)valuePtr, count);
            result = Marshal.PtrToStringAuto((nint)resultPtr) ?? string.Empty;
            Marshal.FreeHGlobal(valuePtr);
        }

        return result;
    }

    private unsafe delegate void MapDelegate(char* key, char* value, char** resultKey, char** resultValue, int* count);

    private unsafe delegate char* ReduceDelegate(char* key, char** values, int count);
}
using System.Security.Cryptography;

namespace QRWells.MapReduce.Utils;

public enum FNVBits
{
    Bits32,
    Bits64
}

public abstract class FNV1a : HashAlgorithm
{
    public static FNV1a Create(FNVBits bits)
    {
        return bits switch
        {
            FNVBits.Bits32 => new FNV1a32(),
            FNVBits.Bits64 => new FNV1a64(),
            _ => throw new NotImplementedException("bits over 64 is not supported")
        };
    }
}

internal sealed class FNV1a32 : FNV1a
{
    private const uint FNV1a32OffsetBasis = 2166136261;
    private const uint FNV1a32Prime = 16777619;

    private uint _hash;

    public FNV1a32()
    {
        Initialize();
    }

    public override void Initialize()
    {
        _hash = FNV1a32OffsetBasis;
    }

    protected override void HashCore(byte[] array, int ibStart, int cbSize)
    {
        for (var i = ibStart; i < cbSize; i++)
        {
            _hash ^= array[i];
            _hash *= FNV1a32Prime;
        }
    }

    protected override byte[] HashFinal()
    {
        return BitConverter.GetBytes(_hash);
    }
}

internal sealed class FNV1a64 : FNV1a
{
    private const ulong FNV1a64OffsetBasis = 14695981039346656037;
    private const ulong FNV1a64Prime = 1099511628211;

    private ulong _hash;

    public FNV1a64()
    {
        Initialize();
    }

    public override void Initialize()
    {
        _hash = FNV1a64OffsetBasis;
    }

    protected override void HashCore(byte[] array, int ibStart, int cbSize)
    {
        for (var i = ibStart; i < cbSize; i++)
        {
            _hash ^= array[i];
            _hash *= FNV1a64Prime;
        }
    }

    protected override byte[] HashFinal()
    {
        return BitConverter.GetBytes(_hash);
    }
}
namespace QRWells.MapReduce.Rpc.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class MethodAttribute : Attribute
{
    public MethodAttribute(string name)
    {
        Name = name;
    }

    public MethodAttribute()
    {
    }

    public string Name { get; }
}
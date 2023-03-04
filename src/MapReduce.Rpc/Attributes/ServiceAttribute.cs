using Microsoft.Extensions.DependencyInjection;

namespace QRWells.MapReduce.Rpc.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class ServiceAttribute : Attribute
{
    public ServiceAttribute(Type contract, ServiceLifetime lifetime = ServiceLifetime.Transient)
    {
        Contract = contract;
        Lifetime = lifetime;
    }

    public ServiceLifetime Lifetime { get; }
    public Type Contract { get; }
}
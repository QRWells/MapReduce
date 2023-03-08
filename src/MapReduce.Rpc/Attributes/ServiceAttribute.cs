using Microsoft.Extensions.DependencyInjection;

namespace QRWells.MapReduce.Rpc.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class ServiceAttribute : Attribute
{
    public ServiceAttribute(Type serviceType, ServiceLifetime lifetime = ServiceLifetime.Transient)
    {
        ServiceType = serviceType;
        Lifetime = lifetime;
    }

    public ServiceLifetime Lifetime { get; set; }
    public Type ServiceType { get; set; }
    public string? Name { get; set; }
}
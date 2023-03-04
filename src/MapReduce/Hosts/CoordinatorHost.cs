using Microsoft.Extensions.Hosting;

namespace QRWells.MapReduce.Hosts;

public class CoordinatorHost : IHostedService
{
    private readonly CoordinatorConfig _config;
    private Coordinator _coordinator;

    public CoordinatorHost(CoordinatorConfig config)
    {
        _config = config;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}

public class CoordinatorConfig
{
    public Action<Coordinator> Configure { get; init; }
}
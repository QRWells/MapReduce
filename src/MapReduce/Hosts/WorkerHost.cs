using Microsoft.Extensions.Hosting;

namespace QRWells.MapReduce.Hosts;

public class WorkerHost : IHostedService
{
    private readonly WorkerConfig _config;
    private Worker _worker;

    public WorkerHost(WorkerConfig config)
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

public class WorkerConfig
{
    public Action<Worker> Configure { get; init; }
}
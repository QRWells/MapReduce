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
        _worker = new Worker();
        _config.Configure?.Invoke(_worker);
        return _worker.Start();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _worker.Dispose();
        return Task.CompletedTask;
    }
}

public class WorkerConfig
{
    public Action<Worker>? Configure { get; init; }
}
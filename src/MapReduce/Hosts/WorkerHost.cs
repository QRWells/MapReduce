using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace QRWells.MapReduce.Hosts;

public class WorkerHost : IHostedService
{
    private readonly WorkerConfig _config;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly ILogger<WorkerHost> _logger;
    private readonly ILogger<Worker> _workerLogger;
    private Worker _worker;

    public WorkerHost(WorkerConfig config, ILoggerFactory loggerFactory, IHostApplicationLifetime lifetime)
    {
        _config = config;
        _logger = loggerFactory.CreateLogger<WorkerHost>();
        _workerLogger = loggerFactory.CreateLogger<Worker>();
        _lifetime = lifetime;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _worker = new Worker(_workerLogger);
        _worker.WorkerStopped += (_, _) => _lifetime.StopApplication();
        _config.Configure?.Invoke(_worker);
        _logger.LogInformation("Starting worker");
        return _worker.Start();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _worker.Dispose();
        _logger.LogInformation("Stopping worker");
        return Task.CompletedTask;
    }
}

public class WorkerConfig
{
    public Action<Worker>? Configure { get; init; }
}
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using QRWells.MapReduce.Method;
using QRWells.MapReduce.Rpc.Client;
using QRWells.MapReduce.Utils;

namespace QRWells.MapReduce;

public class Worker : IDisposable
{
    private readonly ICoordinator _coordinator;
    private readonly ILogger<Worker> _logger;
    private readonly RpcClient _rpcClient;
    private bool _isRunning;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
        _rpcClient = new RpcClient(Host, Port);
        _coordinator = _rpcClient.GetService<ICoordinator>();
    }

    public bool IsRunning => _isRunning;

    public Guid Id { get; } = Guid.NewGuid();
    public HashAlgorithm Hasher { get; set; } = FNV1a.Create(FNVBits.Bits32);

    public int Port { get; set; } = 8080;
    public string Host { get; set; } = "localhost";
    public MethodProxy MethodProxy { get; set; }

    public void Dispose()
    {
        Volatile.Write(ref _isRunning, false);

        _rpcClient.Dispose();

        GC.SuppressFinalize(this);
    }

    public event EventHandler<int>? MapTaskCompleted;
    public event EventHandler<int>? ReduceTaskCompleted;
    public event EventHandler? WorkerStarted;
    public event EventHandler? WorkerStopped;

    private async void RunInternal()
    {
        while (_isRunning)
        {
            var task = await _coordinator.RequestTask();
            _logger.LogInformation("Received task {} with type {}", task.TaskId, task.Type);
            switch (task.Type)
            {
                case TaskType.Map:
                    var res = DoMapTask(task);
                    await _coordinator.CompleteMapTask(task.TaskId, res);
                    MapTaskCompleted?.Invoke(this, task.TaskId);
                    _logger.LogInformation("Completed map task {}", task.TaskId);
                    break;
                case TaskType.Reduce:
                    DoReduceTask(task);
                    await _coordinator.CompleteReduceTask(task.TaskId);
                    ReduceTaskCompleted?.Invoke(this, task.TaskId);
                    _logger.LogInformation("Completed reduce task {}", task.TaskId);
                    break;
                case TaskType.None:
                    _isRunning = false;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        WorkerStopped?.Invoke(this, EventArgs.Empty);
    }

    public Task Start()
    {
        _isRunning = true;
        WorkerStarted?.Invoke(this, EventArgs.Empty);
        new Thread(RunInternal).Start();
        return Task.CompletedTask;
    }

    private IEnumerable<int> DoMapTask(TaskResult task)
    {
        var reduceIds = new List<int>();
        var reduceFile = new Dictionary<uint, FileStream>();
        var writers = new Dictionary<uint, StreamWriter>();

        var reader = new StreamReader(task.File);
        var content = reader.ReadToEnd();
        reader.Close();

        var kv = MethodProxy.Map(task.File, content);

        foreach (var (key, value) in kv)
        {
            var reduce = Hash(key) % task.NumberReduce;
            if (!reduceFile.ContainsKey(reduce))
            {
                reduceFile[reduce] =
                    new FileStream($"mr-{task.TaskId}-{reduce}", FileMode.OpenOrCreate, FileAccess.Write);
                writers[reduce] = new StreamWriter(reduceFile[reduce]);
            }

            writers[reduce].WriteLine($"{key} {value}");
        }

        for (uint i = 0; i < task.NumberReduce; i++)
        {
            reduceIds.Add((int)i);
            writers[i].Close();
        }

        return reduceIds;
    }

    private void DoReduceTask(TaskResult task)
    {
        var intermediate = new List<KeyValuePair<string, string>>();
        foreach (var mapId in task.Keys)
        {
            using var file = new FileStream($"mr-{mapId}-{task.TaskId}", FileMode.Open, FileAccess.Read);
            using var reader = new StreamReader(file);
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var split = line.Split(' ');
                var kv = new KeyValuePair<string, string>(split[0], split[1]);
                intermediate.Add(kv);
            }
        }

        intermediate.Sort((x, y) =>
            string.Compare(x.Key, y.Key, StringComparison.Ordinal));

        var tempName = Path.GetFileName(Path.GetTempFileName());
        var output = new FileStream(tempName, FileMode.Create);
        var writer = new StreamWriter(output);

        var i = 0;

        while (i < intermediate.Count)
        {
            var j = i + 1;
            while (j < intermediate.Count && intermediate[j].Key == intermediate[i].Key) j++;

            var values = new List<string>();
            for (var k = i; k < j; k++) values.Add(intermediate[k].Value);

            var result = MethodProxy.Reduce(intermediate[i].Key, values);
            writer.WriteLine($"{intermediate[i].Key} {result}");
            i = j;
        }

        writer.Close();

        // rename temp file to final file
        File.Move(tempName, $"mr-out-{task.TaskId}");
    }

    private uint Hash(string key)
    {
        var hash = Hasher.ComputeHash(Encoding.UTF8.GetBytes(key));
        return BitConverter.ToUInt32(hash);
    }
}
using System.Security.Cryptography;
using System.Text;
using QRWells.MapReduce.Method;
using QRWells.MapReduce.Rpc.Client;
using QRWells.MapReduce.Utils;

namespace QRWells.MapReduce;

public class Worker : IDisposable
{
    private readonly ICoordinator _coordinator;
    private readonly RpcClient _rpcClient;
    private bool _isRunning;

    public Worker()
    {
        _rpcClient = new RpcClient(Host, Port);
        _coordinator = _rpcClient.GetService<ICoordinator>();
    }

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

    public async Task Start()
    {
        while (_isRunning)
        {
            var task = await _coordinator.RequestTask();
            switch (task.Type)
            {
                case TaskType.Map:
                    var res = DoMapTask(task);
                    await _coordinator.CompleteMapTask(task.TaskId, res);
                    break;
                case TaskType.Reduce:
                    DoReduceTask(task);
                    await _coordinator.CompleteReduceTask(task.TaskId);
                    break;
                case TaskType.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    private IEnumerable<int> DoMapTask(TaskResult task)
    {
        var reduceIds = new List<int>();
        var reduceFile = new Dictionary<uint, FileStream>();

        var reader = new StreamReader(task.File);
        var content = reader.ReadToEnd();
        reader.Close();

        var kv = MethodProxy.Map(task.File, content);

        foreach (var (key, value) in kv)
        {
            var reduce = Hash(key) % task.NumberReduce;
            if (!reduceFile.ContainsKey(reduce))
                reduceFile[reduce] = new FileStream($"mr-{task.TaskId}-{reduce}", FileMode.Create);

            var writer = new StreamWriter(reduceFile[reduce]);
            writer.WriteLine($"{key} {value}");
            writer.Close();
        }

        foreach (var (_, o) in reduceFile)
        {
            o.Close();
            reduceIds.Add((int)o.Length);
        }

        return reduceIds;
    }

    private void DoReduceTask(TaskResult task)
    {
        var intermediate = new List<KeyValuePair<string, string>>();
        foreach (var mapId in task.Keys)
        {
            var file = new FileStream($"mr-{mapId}-{task.TaskId}", FileMode.Open);
            var reader = new StreamReader(file);
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var split = line.Split(' ');
                var kv = new KeyValuePair<string, string>(split[0], split[1]);
                intermediate.Add(kv);
            }

            reader.Close();
            file.Close();
        }

        intermediate.Sort((x, y) => string.Compare(x.Key, y.Key, StringComparison.Ordinal));

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
        output.Close();

        // rename temp file to final file
        File.Move(tempName, $"mr-out-{task.TaskId}");
    }

    private uint Hash(string key)
    {
        var hash = Hasher.ComputeHash(Encoding.UTF8.GetBytes(key));
        return BitConverter.ToUInt32(hash);
    }
}
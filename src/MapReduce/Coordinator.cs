using Microsoft.Extensions.DependencyInjection;
using QRWells.MapReduce.Hosts;
using QRWells.MapReduce.Rpc.Attributes;

namespace QRWells.MapReduce;

[Service(typeof(ICoordinator), ServiceLifetime.Singleton)]
public class Coordinator : ICoordinator
{
    private const int TaskStatusIdle = 0;
    private const int TaskStatusRunning = 1;
    private const int TaskStatusCompleted = 2;

    private static readonly KeyValuePair<int, MapTask> EmptyMapTask = new(-1, new MapTask());
    private static readonly KeyValuePair<int, ReduceTask> EmptyReduceTask = new(-1, new ReduceTask());
    private static readonly NoTaskResult NoTaskResult = new();
    private readonly object _lock = new();
    private readonly Dictionary<int, MapTask> _mapTasks = new();
    private readonly uint _numberReduce;
    private readonly Dictionary<int, ReduceTask> _reduceTasks = new();

    private int _timeout = 5000;

    public Coordinator(CoordinatorConfig config)
    {
        _numberReduce = config.NumberReduce;
        foreach (var file in config.Files)
        {
            var mapTask = new MapTask
            {
                Status = TaskStatusIdle,
                File = file
            };
            _mapTasks.Add(_mapTasks.Count, mapTask);
        }

        config.Configure?.Invoke(this);
    }

    public Task<TaskResult> RequestTask()
    {
        Monitor.Enter(_lock);

        var task = Task.FromResult<TaskResult>(NoTaskResult);

        if (_mapTasks.Count > 0)
        {
            var mapTask =
                _mapTasks.FirstOrDefault(x => x.Value.Status == TaskStatusIdle, EmptyMapTask);
            if (mapTask.Key != -1)
            {
                mapTask.Value.Status = TaskStatusRunning;
                task = Task.FromResult(new TaskResult
                {
                    TaskId = mapTask.Key,
                    NumberReduce = _numberReduce,
                    File = mapTask.Value.File
                });

                Task.Run(async () =>
                {
                    async Task Timeout(int key)
                    {
                        await Task.Delay(_timeout);
                        Monitor.Enter(_lock);
                        if (_mapTasks.TryGetValue(key, out var value)) value.Status = TaskStatusIdle;

                        Monitor.Exit(_lock);
                    }

                    await Timeout(mapTask.Key);
                });
            }
        }
        else if (_reduceTasks.Count > 0)
        {
            var reduceTask =
                _reduceTasks.FirstOrDefault(x => x.Value.Status == TaskStatusIdle, EmptyReduceTask);
            if (reduceTask.Key != -1)
            {
                reduceTask.Value.Status = TaskStatusRunning;
                task = Task.FromResult(new TaskResult
                {
                    TaskId = reduceTask.Key,
                    Keys = reduceTask.Value.Keys
                });

                Task.Run(async () =>
                {
                    async Task Timeout(int key)
                    {
                        await Task.Delay(_timeout);
                        Monitor.Enter(_lock);
                        if (_reduceTasks.TryGetValue(key, out var value)) value.Status = TaskStatusIdle;

                        Monitor.Exit(_lock);
                    }

                    await Timeout(reduceTask.Key);
                });
            }
        }

        Monitor.Exit(_lock);

        return task;
    }

    public Task CompleteMapTask(int taskId, IEnumerable<int> results)
    {
        Monitor.Enter(_lock);
        _mapTasks.Remove(taskId);
        foreach (var result in results)
        {
            if (!_reduceTasks.TryGetValue(result, out var reduceTask))
            {
                reduceTask = new ReduceTask
                {
                    Status = TaskStatusIdle,
                    Keys = new List<int>()
                };
                _reduceTasks.Add(result, reduceTask);
            }

            ((List<int>)reduceTask.Keys).Add(taskId);
        }

        Monitor.Exit(_lock);
        return Task.CompletedTask;
    }

    public Task CompleteReduceTask(int taskId)
    {
        Monitor.Enter(_lock);
        _reduceTasks.Remove(taskId);
        Monitor.Exit(_lock);
        return Task.CompletedTask;
    }

    [Ignore]
    public Coordinator SetTimeout(uint timeout)
    {
        _timeout = (int)timeout;
        return this;
    }
}

public class MapTask
{
    public int Status { get; set; }
    public string File { get; set; }
}

public class ReduceTask
{
    public int Status { get; set; }
    public IEnumerable<int> Keys { get; set; }
}

public interface ICoordinator
{
    Task<TaskResult> RequestTask();
    Task CompleteMapTask(int taskId, IEnumerable<int> results);
    Task CompleteReduceTask(int taskId);
}

public class TaskResult
{
    public TaskType Type { get; }
    public int TaskId { get; set; }
    public uint NumberReduce { get; set; }
    public string File { get; set; }
    public IEnumerable<int> Keys { get; set; }
}

public class NoTaskResult : TaskResult
{
    public TaskType Type => TaskType.None;
    public int TaskId { get; set; } = -1;
}

public enum TaskType
{
    None,
    Map,
    Reduce
}
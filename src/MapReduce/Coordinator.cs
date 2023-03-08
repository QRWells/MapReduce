using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
    private static readonly TaskResult NoTaskResult = new() { TaskId = -1 };
    private readonly object _lock = new();
    private readonly Dictionary<int, MapTask> _mapTasks = new();
    private readonly uint _numberReduce;
    private readonly Dictionary<int, ReduceTask> _reduceTasks = new();
    private readonly List<Task> _tasks = new();

    private int _timeout = 5000;

    public Coordinator(CoordinatorConfig config, IHostApplicationLifetime lifetime)
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

        AllTasksCompleted += (_, _) => lifetime.StopApplication();

        config.Configure?.Invoke(this);
    }

    public Task<TaskResult> RequestTask()
    {
        Monitor.Enter(_lock);

        var task = Task.FromResult(NoTaskResult);

        if (_mapTasks.Count > 0)
        {
            var mapTask =
                _mapTasks.FirstOrDefault(x => x.Value.Status == TaskStatusIdle, EmptyMapTask);
            if (mapTask.Key != -1)
            {
                mapTask.Value.Status = TaskStatusRunning;
                task = Task.FromResult(new TaskResult
                {
                    Type = TaskType.Map,
                    TaskId = mapTask.Key,
                    NumberReduce = _numberReduce,
                    File = mapTask.Value.File
                });

                var t = Task.Run(async () =>
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
                _tasks.Add(t);
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
                    Type = TaskType.Reduce,
                    TaskId = reduceTask.Key,
                    Keys = reduceTask.Value.Keys
                });

                var t = Task.Run(async () =>
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
                _tasks.Add(t);
            }
        }

        _tasks.RemoveAll(t => t.IsCompleted);

        if (_mapTasks.Count == 0 && _reduceTasks.Count == 0 && _tasks.Count == 0)
            AllTasksCompleted?.Invoke(this, EventArgs.Empty);

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

    public event EventHandler? AllTasksCompleted;

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
    public TaskType Type { get; set; }
    public int TaskId { get; set; }
    public uint NumberReduce { get; set; }
    public string File { get; set; }
    public IEnumerable<int> Keys { get; set; }
}

public enum TaskType
{
    None,
    Map,
    Reduce
}
using QRWells.MapReduce.Rpc.Client;

namespace QRWells.MapReduce;

public class Worker : IDisposable
{
    private ICoordinator _coordinator;
    private bool _isRunning;
    private RpcClient _rpcClient;

    public IMapper Mapper { get; set; }
    public IReducer Reducer { get; set; }

    public async Task Start()
    {
        while (_isRunning)
        {
            var task = await _coordinator.RequestTask();
            switch (task.Type)
            {
                case TaskType.Map:
                    break;
                case TaskType.Reduce:
                    break;
                case TaskType.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    private void DoMapTask()
    {
        throw new NotImplementedException();
    }

    private void DoReduceTask()
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        Volatile.Write(ref _isRunning, false);
    }
}
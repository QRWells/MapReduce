namespace QRWells.MapReduce.Hosts;

public class CoordinatorConfig
{
    public IEnumerable<string> Files { get; init; }
    public int NumberReduce { get; init; }
    public Action<Coordinator> Configure { get; init; }
}
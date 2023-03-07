namespace QRWells.MapReduce.Hosts;

public class CoordinatorConfig
{
    public IEnumerable<string> Files { get; init; }
    public uint NumberReduce { get; init; }
    public Action<Coordinator>? Configure { get; init; }
}
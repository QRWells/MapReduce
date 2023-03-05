using CommandLine;

namespace QRWells.MapReduce;

[Verb("coordinator", aliases: new[] { "master" }, Hidden = false)]
public class CoordinatorOptions
{
    [Option('p', "port", Required = false, HelpText = "Set port of the coordinator.")]
    public int Port { get; set; } = 8080;

    [Option('t', "timeout", Required = false, HelpText = "Set timeout of the worker.")]
    public int Timeout { get; set; } = 5000;

    [Option('r', "reduce", Required = true, HelpText = "Set number of reduce.")]
    public int NumberReduce { get; set; } = 1;

    [Option('f', "files", Required = true, HelpText = "Set files of the coordinator.")]
    public IEnumerable<string> Files { get; set; } = new List<string>();
}

[Verb("worker", true, Hidden = false)]
public class WorkerOptions
{
    [Option('h', "host", Required = true, HelpText = "Set host of the coordinator.")]
    public string Host { get; set; }

    [Option('p', "port", Required = false, HelpText = "Set port of the coordinator.")]
    public int Port { get; set; } = 8080;

    [Option('t', "task", Required = true, HelpText = "Set task of the worker.")]
    public string Task { get; set; } = "map_reduce.dll";

    [Option('t', "timeout", Required = false, HelpText = "Set timeout of the worker.")]
    public int Timeout { get; set; } = 5000;
}

public class RuntimeArgs
{
    public string? CoordinatorHost { get; set; } = "localhost";
    public int Port { get; set; } = 8080;
    public bool IsCoordinator { get; set; }
    public int Timeout { get; set; } = 5000;
    public int NumberReduce { get; set; } = 1;
    public string Task { get; set; } = "map_reduce.dll";
    public IEnumerable<string> Files { get; set; }
}
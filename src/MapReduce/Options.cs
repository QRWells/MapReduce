using CommandLine;

namespace QRWells.MapReduce;

[Verb("coordinator", aliases: new[] { "master" }, Hidden = false)]
public class CoordinatorOptions
{
    [Option('p', "port", Default = 8080, Required = false, HelpText = "Set port of the coordinator.")]
    public int Port { get; set; }

    [Option('t', "timeout", Required = false, HelpText = "Set timeout of the worker.")]
    public int Timeout { get; set; }

    [Option('r', "reduce", Required = true, HelpText = "Set number of reduce.")]
    public int NumberReduce { get; set; }

    [Option('f', "files", Required = true, HelpText = "Set files of the coordinator.")]
    public IEnumerable<string> Files { get; set; }
}

[Verb("worker", true, Hidden = false)]
public class WorkerOptions
{
    [Option('h', "host", Required = true, HelpText = "Set host of the coordinator.")]
    public string Host { get; set; }

    [Option('p', "port", Default = 8080, Required = false, HelpText = "Set port of the coordinator.")]
    public int Port { get; set; }

    [Option('w', "task", Required = true, HelpText = "Set task of the worker.")]
    public string Task { get; set; }

    [Option('t', "timeout", Required = false, HelpText = "Set timeout of the worker.")]
    public int Timeout { get; set; }
}

public class RuntimeArgs
{
    public string? CoordinatorHost { get; set; } = "localhost";
    public int Port { get; set; } = 8080;
    public bool IsCoordinator { get; set; }
    public uint Timeout { get; set; } = 5000;
    public uint NumberReduce { get; set; } = 1;
    public string Task { get; set; }
    public IEnumerable<string> Files { get; set; }
}
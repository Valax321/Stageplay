using System.Reflection;
using CommandLine;
using JetBrains.Annotations;

namespace Radish.ContentBuilder;

[UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
public abstract class BuilderTask
{
    [Option('c', "content-path", HelpText = "Path to the game content directory. Defaults to <cwd>/content if not specified.")]
    public string ContentPath { get; set; } = string.Empty;

    [Option('o', "output-path", HelpText = "Path to the cooked data directory. Defaults to <cwd>/cooked if not specified.")]
    public string OutputPath { get; set; } = string.Empty;
    
    [Option('v', "verbose", HelpText = "Enable verbose logging.")]
    public bool Verbose { get; set; }

    public const int Success = 0;
    
    public static IEnumerable<Type> GetAll() => Assembly.GetEntryAssembly()!
        .GetTypes()
        .Where(x => x.IsSubclassOf(typeof(BuilderTask)))
        .Where(x => x.GetCustomAttribute<VerbAttribute>() != null);

    public void Configure()
    {
        if (string.IsNullOrEmpty(ContentPath))
            ContentPath = Path.Combine(Environment.CurrentDirectory, "content");

        if (string.IsNullOrEmpty(OutputPath))
            OutputPath = Path.Combine(Environment.CurrentDirectory, "cooked");
        
        Console.WriteLine($"Content path: {ContentPath}");
        Console.WriteLine($"Output path: {OutputPath}");
        Console.WriteLine();
    }
    
    public abstract Task<int> RunAsync();
}
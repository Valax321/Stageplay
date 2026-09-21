using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using CommandLine;
using JetBrains.Annotations;

namespace Radish.ContentBuilder;

/// <summary>
/// Represents a verb executable by the content builder application.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
public abstract class BuilderTask
{
    /// <summary>
    /// The path to the game content directory where uncooked assets are stored.
    /// </summary>
    [Option('c', "content-path", HelpText = "Path to the game content directory. Defaults to <cwd>/content if not specified.")]
    public string ContentPath { get; set; } = string.Empty;

    /// <summary>
    /// The path to the output directory where processed assets are stored. Contents of this directory are directly consumable by the runtime.
    /// </summary>
    [Option('o', "output-path", HelpText = "Path to the processed asset directory. Defaults to <cwd>/cooked if not specified.")]
    public string OutputPath { get; set; } = string.Empty;
    
    /// <summary>
    /// If <see langword="true"/> then verbose logging will be used.
    /// </summary>
    [Option('v', "verbose", HelpText = "Enable verbose logging.")]
    public bool Verbose { get; set; }

    /// <summary>
    /// Represents a non-error process exit code.
    /// </summary>
    public const int Success = 0;
    
    /// <summary>
    /// Runs the verb and returns an exit code describing the outcome.
    /// </summary>
    /// <returns>A process exit code indicating the outcome of the task.</returns>
    public abstract Task<int> RunAsync();
    
    [RequiresDynamicCode("Uses reflection to get all BuilderTask types from the loaded assemblies.")]
    internal static IEnumerable<Type> GetAllTasks() => AppDomain.CurrentDomain.GetAssemblies()
        .SelectMany(x => x.GetExportedTypes())
        .Where(x => x.IsSubclassOf(typeof(BuilderTask)))
        .Where(x => x.GetCustomAttribute<VerbAttribute>() != null);

    internal void Configure()
    {
        if (string.IsNullOrEmpty(ContentPath))
            ContentPath = Path.Combine(Environment.CurrentDirectory, "content");

        if (string.IsNullOrEmpty(OutputPath))
            OutputPath = Path.Combine(Environment.CurrentDirectory, "cooked");
        
        Console.WriteLine($"Content path: {ContentPath}");
        Console.WriteLine($"Output path: {OutputPath}");
        Console.WriteLine();
    }
}
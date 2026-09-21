using System.Diagnostics.CodeAnalysis;
using CommandLine;
using JetBrains.Annotations;

namespace Radish.ContentBuilder;

/// <summary>
/// Parses the program's command line parameters and runs the requested verb.
/// Also handles printing the help screen if requested by the user.
/// </summary>
[PublicAPI]
public static class Runner
{
    /// <summary>
    /// Runs the content builder with the requested verb, or shows the generated help screen.
    /// </summary>
    /// <param name="args">The command line arguments passed to the program.</param>
    [RequiresDynamicCode("Uses reflection to get all BuilderTask types from the loaded assemblies.")]
    public static async Task Run(string[] args)
    {
        await Parser.Default.ParseArguments(args, [.. BuilderTask.GetAllTasks()])
            .WithParsedAsync<BuilderTask>(async b =>
            {
                b.Configure();
                Environment.ExitCode = await b.RunAsync();
            });
    }
}
using CommandLine;
using JetBrains.Annotations;

namespace Radish.ContentBuilder;

[PublicAPI]
public static class Runner
{
    public static async Task Run(string[] args)
    {
        await Parser.Default.ParseArguments(args, [.. BuilderTask.GetAll()])
            .WithParsedAsync<BuilderTask>(async b =>
            {
                b.Configure();
                Environment.ExitCode = await b.RunAsync();
            });
    }
}
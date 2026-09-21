using JetBrains.Annotations;

namespace Radish.ContentBuilder;

/// <summary>
/// Implements a verb to process all content files in a directory into game assets.
/// </summary>
/// <typeparam name="TRecipe">A type implementing <see cref="ICookRecipe"/> describing the processors that need to be invoked.</typeparam>
[PublicAPI]
public class CookTask<TRecipe> : BuilderTask
    where TRecipe : ICookRecipe, new()
{
    private readonly ICookRecipe _recipe;

    /// <summary>
    /// Creates a new cook task containing the provided recipe type.
    /// </summary>
    protected CookTask()
    {
        _recipe = new TRecipe
        {
            Task = this
        };
    }
    
    /// <inheritdoc/>
    public override async Task<int> RunAsync()
    {
        var queue = new ProcessorQueue(new DirectoryInfo(ContentPath), new DirectoryInfo(OutputPath));
        BuildProcessorJobQueue(queue);

        var result = await queue.ExecuteAll();

        Console.WriteLine();
        Console.WriteLine($"{result.TotalProcessed} job(s) run with {result.Failed.Count} errors");

        return result.Failed.Count > 0 ? 1 : Success;
    }

    private void BuildProcessorJobQueue(ProcessorQueue queue)
    {
        _recipe.BuildProcessorQueue(queue);
    }
}
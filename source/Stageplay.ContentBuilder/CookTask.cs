namespace Radish.ContentBuilder;

public class CookTask<TRecipe> : BuilderTask
    where TRecipe : ICookRecipe, new()
{
    private readonly ICookRecipe _recipe;

    public CookTask()
    {
        _recipe = new TRecipe
        {
            Task = this
        };
    }
    
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
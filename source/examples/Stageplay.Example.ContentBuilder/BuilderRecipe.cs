using CommandLine;
using Radish.ContentBuilder;
using Radish.ContentBuilder.StandardAssetProcessors;

namespace Radish;

public class StageplayExampleRecipe : ICookRecipe
{
    // We promise, .NET just won't let me use required in interfaces for some reason.
    public BuilderTask Task { get; init; } = null!;
    
    public void BuildProcessorQueue(ProcessorQueue queue)
    {
        // The runtime expects all Lua scripts to be in the scripts/ folder, so only look there.
        queue.AddByGlobPattern("scripts/**/*.lua", 
            new LuaBytecodeProcessor());
        
        // Images are allowed anywhere in the content path.
        // Handle some common formats.
        queue.AddByGlobPattern([
            "**/*.png",
            "**/*.jpg"
        ], new TextureProcessor());
    }
}

[Verb("build", HelpText = "Builds all assets for the game.")]
public class ExampleCookTask : CookTask<StageplayExampleRecipe>;

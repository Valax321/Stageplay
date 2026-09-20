using CommandLine;
using Radish.ContentBuilder;
using Radish.Lua;

namespace Radish;

public class StageplayRecipe : ICookRecipe
{
    // We promise, .NET just won't let me use required in interfaces for some reason.
    public BuilderTask Task { get; init; } = null!;
    
    public void BuildProcessorQueue(ProcessorQueue queue)
    {
        queue.AddByGlobPattern("scripts/**/*.lua", 
            new LuaCompilerProcessor());
    }
}

[Verb("build", HelpText = "Builds all assets for the game.")]
public class MonoGameCookTask : CookTask<StageplayRecipe>;

using Radish.Content;

namespace Radish;

public abstract class Game
{
    public StageplayRuntime Runtime { get; init; } = null!;
    
    public virtual void MountContent(ContentManager content)
    {}
    
    public virtual void PostStartup()
    {}
    
    public virtual void PreShutdown()
    {}
}
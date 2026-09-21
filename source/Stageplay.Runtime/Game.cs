namespace Radish;

public abstract class Game
{
    public virtual void MountContent(IContentManager content)
    {}
    
    public virtual void PostRuntimeInit()
    {}
}
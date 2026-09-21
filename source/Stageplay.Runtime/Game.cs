using JetBrains.Annotations;
using Radish.Content;

namespace Radish;

/// <summary>
/// Game-specific base class that allows for implementing some custom per-game functionality in the runtime.
/// Used to mount archive files, implement custom C# behaviour, etc.
/// </summary>
[PublicAPI]
public abstract class Game
{
    /// <summary>
    /// The runtime that owns this game.
    /// </summary>
    public StageplayRuntime Runtime { get; init; } = null!;
    
    /// <summary>
    /// Called when the filesystem is initialising. Used to mount archive files via <see cref="ContentManager.AddFsArcFile"/>.
    /// </summary>
    /// <param name="content">The content manager that is initialising.</param>
    public virtual void MountContent(ContentManager content)
    {}
    
    /// <summary>
    /// Callback invoked when the runtime has completed startup.
    /// </summary>
    public virtual void PostStartup()
    {}
    
    /// <summary>
    /// Callback invoked at runtime shutdown, before any framework systems have terminated.
    /// </summary>
    public virtual void PreShutdown()
    {}
}
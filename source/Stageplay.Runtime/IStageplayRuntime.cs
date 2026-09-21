using Foster.Framework;

namespace Radish;

public interface IStageplayRuntime : IDisposable
{
    IServiceProvider Services { get; }
    
    GraphicsDevice? GraphicsDevice { get; }

    void Run();
}

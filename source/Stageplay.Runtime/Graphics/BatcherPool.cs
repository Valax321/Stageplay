using Foster.Framework;
using JetBrains.Annotations;

namespace Radish.Graphics;

/// <summary>
/// Provides object pooling for <see cref="Batcher"/> instances..
/// Batchers are automatically cleared when returned, so it does not need to be done manually.
/// </summary>
/// <param name="graphicsDevice">The graphics device to use when creating new batcher instances.</param>
[PublicAPI]
public sealed class BatcherPool(GraphicsDevice graphicsDevice) : IDisposable
{
    private readonly Queue<Batcher> _batchers = new(32);
    
    public Batcher Get()
    {
        if (_batchers.TryDequeue(out var b))
        {
            return b;
        }

        return new Batcher(graphicsDevice);
    }

    public void Return(Batcher batcher)
    {
        batcher.Clear();
        _batchers.Enqueue(batcher);
    }

    public void Dispose()
    {
        foreach (var b in _batchers)
            b.Dispose();
    }
}
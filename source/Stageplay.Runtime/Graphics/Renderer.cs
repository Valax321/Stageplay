using System.Numerics;
using Foster.Framework;
using Radish.Debugger;

namespace Radish.Graphics;

internal sealed class Renderer(GraphicsDevice graphicsDevice, DebugMenu debugUI) : IDisposable
{
    private readonly BatcherPool _batcherPool = new(graphicsDevice);
    
    internal void DrawFrame(Window window)
    {
        window.Clear(Color.Black);
        DrawDebugUI(window);
    }

    private void DrawDebugUI(Window dest)
    {
        var batcher = _batcherPool.Get();
        
        // Ensure the debug menu uses virtual 1280x720 coordinate space
        batcher.PushMatrix(Vector2.Zero, Vector2.One * dest.ContentScale.Y, 0);
        
        var aspect = (float)dest.WidthInPixels / dest.HeightInPixels;
        debugUI.Draw(batcher, new RectInt(Point2.Zero, dest.Size));
        batcher.Render(dest);
        
        _batcherPool.Return(batcher);
    }

    public void Dispose()
    {
        _batcherPool.Dispose();
    }
}
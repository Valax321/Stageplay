using System.Numerics;
using Foster.Framework;

namespace Radish.Graphics;

internal sealed class Renderer : IDisposable
{
    private readonly StageplayRuntime _app;
    private readonly BatcherPool _batcherPool;

    private readonly LinkedList<RenderScene> _scenes = [];

    public Renderer(StageplayRuntime app)
    {
        _app = app;
        _batcherPool = new BatcherPool(app.GraphicsDevice);

        _scenes.AddLast(new RenderScene(app.GraphicsDevice, app.GameInfo.DesignSize)
        {
            ClearColor = Color.CornflowerBlue
        });
    }

    internal void DrawFrame(Window window)
    {
        window.Clear(Color.Black);
        
        DrawGameView(window);
        DrawDebugUI(window);
    }

    private void DrawGameView(Window window)
    {
        var windowSz = window.SizeInPixels;

        var windowAspect = (float)windowSz.X / windowSz.Y;
        var designSizeAspect = (float)_app.GameInfo.DesignSize.X / _app.GameInfo.DesignSize.Y;

        var compBatch = _batcherPool.Get();
        
        Point2 drawSize;
        float drawScale;

        if (windowAspect >= designSizeAspect)
        {
            drawSize = new Point2((int)(windowSz.Y * designSizeAspect), windowSz.Y);
            drawScale = (float)windowSz.Y / _app.GameInfo.DesignSize.Y;
        }
        else
        {
            var oneOverAspect = 1 / designSizeAspect;
            drawSize = new Point2(windowSz.X, (int)(windowSz.Y * oneOverAspect));
            drawScale = (float)windowSz.X / _app.GameInfo.DesignSize.X;
        }
        
        var sceneDrawPos = new Point2(
            (int)((windowSz.X - drawSize.X) / 2.0),
            (int)((windowSz.Y - drawSize.Y) / 2.0)
        );
        
        compBatch.PushBlend(BlendMode.NonPremultiplied);

        // Draw each scene to its target, and build the compositor batch command for drawing
        // that scene to the backbuffer
        foreach (var scene in _scenes)
        {
            var batch = _batcherPool.Get();
            batch.PushMatrix(Vector2.Zero, Vector2.One * drawScale, 0);
            batch.PushSampler(new TextureSampler(TextureFilter.Linear, TextureWrap.Clamp));
            
            scene.Draw(new RenderScene.FrameParams(drawSize, batch));
            _batcherPool.Return(batch);
            
            // TODO: implement masked drawing (needs custom shader)
            compBatch.ImageStretch(
                new Subtexture(scene.Texture), 
                new Rect(sceneDrawPos, drawSize), 
                scene.BlendColor
            );
        }
        
        compBatch.Render(window);
        _batcherPool.Return(compBatch);
    }

    private void DrawDebugUI(Window dest)
    {
        var batcher = _batcherPool.Get();
        
        // Clamp the UI area to the design size area
        var uiSz = (dest.SizeInPixels / dest.ContentScale.Y).FloorToPoint2();

        // Scales the debug menu according to DPI scale
        batcher.PushMatrix(Vector2.Zero, Vector2.One * dest.ContentScale.Y, 0);

        _app.DebugMenu.Draw(batcher, new RectInt(Point2.Zero, uiSz));
        
        batcher.Render(dest);
        _batcherPool.Return(batcher);
    }

    public static Point2 MakeMaxRenderTargetSize(Point2 windowSize, double aspectRatio)
    {
        // Allow aspect ratios less than 16:9, but anything wider will be clamped
        var aspect = (double)windowSize.X / windowSize.Y;
        if (aspect > aspectRatio)
        {
            aspect = aspectRatio;
        }

        return new Point2((int)Math.Floor(aspect * windowSize.Y), windowSize.Y);
    }

    public void Dispose()
    {
        foreach (var s in _scenes)
            s.Dispose();
        _batcherPool.Dispose();
    }
}
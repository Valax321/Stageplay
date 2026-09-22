using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Foster.Framework;

namespace Radish.Graphics;

internal sealed class RenderScene(GraphicsDevice graphicsDevice, Point2 designSize) : IDisposable
{
    public readonly record struct FrameParams(Point2 RenderTargetSize, Batcher Batch);

    public Color ClearColor { get; set; } = Color.Black;
    
    public Target Texture => _sceneTarget 
                             ?? throw new InvalidOperationException("Target texture not yet initialized. Call Draw() at least once.");
    
    public Texture? MaskTexture { get; set; }
    public Color BlendColor { get; set; } = Color.White;

    private Target? _sceneTarget;

    public void Draw(in FrameParams frame)
    {
        ReallocTargetWithDesiredSize(ref _sceneTarget, frame.RenderTargetSize, 
            (g, s) => new Target(g, s.X, s.Y, name: "RenderScene Target")
        );
        
        _sceneTarget.Clear(ClearColor);
        DrawSceneContents(frame.Batch);
        frame.Batch.Render(_sceneTarget);
    }

    private void DrawSceneContents(Batcher batch)
    {
        batch.Text("Hello", new Vector2(10, 10), 32, Color.Red);
    }
    
    public void Dispose()
    {
        _sceneTarget?.Dispose();
        _sceneTarget = null;
    }
    
    private void ReallocTargetWithDesiredSize([NotNull] ref Target? target, Point2 desiredSize,
        Func<GraphicsDevice, Point2, Target> factory)
    {
        if (target is not null && desiredSize.Equals(new Point2(target.Width, target.Height)))
            return;

        target?.Dispose();
        target = factory(graphicsDevice, desiredSize);
    }
}
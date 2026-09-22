using System.Diagnostics;
using System.Numerics;
using Cysharp.Text;
using Foster.Framework;

namespace Radish.Debugger;

internal sealed class MenuContainer(string name)
{
    public record MenuItem(string Name, Action<MenuContainer>? SelectedCallback);

    public List<MenuItem> Items { get; init; } = [];
    public int SelectedIndex { get; private set; }
    public MenuContainer? Parent { get; set; }
    public Action? MenuClosed { get; init; }

    private MenuContainer? _childMenu;

    private static readonly Color BackgroundColor = new(39, 50, 73, 255);

    public void Draw(in DebugMenu.DrawInfo draw)
    {
        var vpH = (48 + ((int)draw.ScaledTextLineHeight * Items.Count));
        var colMul = (_childMenu != null ? 0.5f : 1.0f);
        
        draw.Batch.PushBlend(BlendMode.NonPremultiplied);
        draw.Batch.Rect(draw.Viewport with { Height = vpH }, BackgroundColor.ScaleV(colMul));
        draw.Batch.RectLine(draw.Viewport with { Height = vpH }, 1.0f, Color.White.ScaleV(colMul));
        draw.Batch.RectLine((draw.Viewport with { Height = vpH }).Inflate(1), 2.0f, (Color.Black with { A = 68 }).ScaleV(colMul));
        draw.Batch.PopBlend();

        var cursorPos = draw.Viewport.TopCenter + new Vector2(0, 4);
        
        draw.Batch.Text(draw.Font, name, 
            cursorPos, TextJustify.Centre, 
            DebugMenu.BaseFontSize, Color.White
        );

        cursorPos.Y += draw.ScaledTextLineHeight * 1.5f;
        cursorPos.X = draw.Viewport.Left + 4;

        var nextMenuY = 0;

        for (var i = 0; i < Items.Count; ++i)
        {
            using var sb = ZString.CreateStringBuilder();
            sb.Append(i == SelectedIndex ? "> " : "  ");

            var item = Items[i];
            sb.Append(item.Name);
            draw.Batch.Text(draw.Font, sb.AsSpan(), cursorPos, DebugMenu.BaseFontSize, Color.White);

            if (i == SelectedIndex)
                nextMenuY = (int)cursorPos.Y;
            
            cursorPos.Y += draw.ScaledTextLineHeight;
        }

        if (_childMenu is not null)
        {
            var subMenuRect = draw.Viewport with { Y = nextMenuY } + new Point2(16, 16);
            _childMenu.Draw(draw with { Viewport = subMenuRect });
        }
        else
        {
            ProcessInput(draw.Input);
        }
    }

    private void ProcessInput(in NavInputs nav)
    {
        if (nav.Up)
        {
            SelectedIndex = (SelectedIndex - 1) % Items.Count;
            if (SelectedIndex < 0)
                SelectedIndex += Items.Count;
        }

        if (nav.Down)
        {
            SelectedIndex = (SelectedIndex + 1) % Items.Count;
        }

        if (nav.Cancel)
        {
            Parent?.CloseSubMenu();
            MenuClosed?.Invoke();
            return;
        }

        if (nav.Select)
        {
            Debug.Assert(SelectedIndex >= 0 && SelectedIndex < Items.Count);
            var item = Items[SelectedIndex];
            item.SelectedCallback?.Invoke(this);
        }
    }

    public void PushSubMenu(MenuContainer menu)
    {
        _childMenu = menu;
        _childMenu.Parent = this;
    }

    public void CloseSubMenu()
    {
        _childMenu?.Parent = null;
        _childMenu = null;
    }
}
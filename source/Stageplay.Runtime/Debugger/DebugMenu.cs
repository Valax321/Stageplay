using System.Numerics;
using System.Runtime.InteropServices;
using Cysharp.Text;
using Foster.Framework;
using SDL3;

namespace Radish.Debugger;

internal sealed class DebugMenu(StageplayRuntime app) : IDebugMenu
{
    public readonly record struct DrawInfo(Batcher Batch, SpriteFont Font, RectInt Viewport, NavInputs Input)
    {
        public float ScaledTextLineHeight
        {
            get
            {
                var scale = (BaseFontSize / Font.Size);
                var lineHeight = Font.LineHeight * scale;
                return lineHeight;
            }
        }
    }

    public const int BaseFontSize = 13;
    public const int MenuWidth = 300;
    public const string LineSepText = "----------------------------------";

    private SpriteFont? _debugFont;

    private MenuContainer? _rootDebugMenu;
    private bool _showRootMenu;

    private static SpriteFont LoadFont(GraphicsDevice graphicsDevice)
    {
        using var img = new Image(Calc.ReadEmbeddedBytes(
            typeof(DebugMenu).Assembly,
            "Fonts/DebugFont.png"
        ));

        var msdfFont = new MsdfFont(
            img,
            Calc.ReadEmbeddedBytes(
                typeof(DebugMenu).Assembly,
                "Fonts/DebugFont.json"
            )
        );

        return new SpriteFont(graphicsDevice, msdfFont);
    }

    internal void Draw(Batcher batch, RectInt viewportSize)
    {
        _debugFont ??= LoadFont(batch.GraphicsDevice);
        var drawInfo = new DrawInfo(batch, _debugFont, viewportSize, NavInputs.FromInput(app.Input));

        DrawStats(in drawInfo);
        DrawRootMenu(in drawInfo);
    }

    private void DrawRootMenu(in DrawInfo draw)
    {
        if (!_showRootMenu)
        {
            if (app.Input.Keyboard.Pressed(Keys.F1))
                _showRootMenu = true;
            return;
        }

        _rootDebugMenu ??= new MenuContainer("Stageplay DebugMenu")
        {
            MenuClosed = OnRootMenuClosed,
            Items =
            [
                new MenuContainer.MenuItem("System", PushSystemMenu),
                new MenuContainer.MenuItem("LuaScript", PushScriptingMenu),
                new MenuContainer.MenuItem("Scenario", PushScenarioMenu)
            ]
        };

        var menuRect = new RectInt(16, 16, MenuWidth, 180);
        _rootDebugMenu.Draw(draw with { Viewport = menuRect });
        DrawControls(in draw);
    }

    private void DrawStats(in DrawInfo draw)
    {
        using var sb = ZString.CreateStringBuilder();
        sb.AppendFormat("Frame: {0:F2}ms / {1:F2}fps", app.Time.Delta * 1000, 1 / app.Time.Delta);
        sb.AppendLine();
        sb.AppendFormat("Mem: {0:F2}MB", Environment.WorkingSet / (1024.0 * 1024.0));
        sb.AppendLine();
        sb.AppendFormat("HeapMem: {0:F2}MB", GC.GetTotalMemory(false) / (1024.0 * 1024.0));

        draw.Batch.Text(draw.Font, sb.AsSpan(),
            draw.Viewport.TopRight + new Point2(-16, 16), TextJustify.Right,
            BaseFontSize, Color.White
        );
    }

    private void DrawControls(in DrawInfo draw)
    {
        var pos = (Vector2)draw.Viewport.BottomLeft;
        pos.Y -= (16 + draw.ScaledTextLineHeight);
        pos.X += 16;

        draw.Batch.Text(draw.Font, "// Arrow Keys | Dpad - navigate menus // F1 | L3 + Start - open debug menu //", pos,
            BaseFontSize, Color.White);
    }

    private void OnRootMenuClosed()
    {
        _showRootMenu = false;
    }

    private void PushSystemMenu(MenuContainer parent)
    {
        parent.PushSubMenu(new MenuContainer("System")
        {
            Items =
            {
                new MenuContainer.MenuItem("Build Info", PushBuildInfoMenu),
                new MenuContainer.MenuItem("HW Info", PushHWInfoMenu),
                new MenuContainer.MenuItem(LineSepText, null),
                new MenuContainer.MenuItem("Force GC Collect", _ => GC.Collect(2)),
                new MenuContainer.MenuItem("Quit Game", _ => app.Exit())
            }
        });
    }

    private void PushHWInfoMenu(MenuContainer parent)
    {
        parent.PushSubMenu(new MenuContainer("HW Info")
        {
            Items =
            {
                new MenuContainer.MenuItem(
                    $"OS: {RuntimeInformation.OSDescription}", null),
                new MenuContainer.MenuItem($"Arch: {RuntimeInformation.ProcessArchitecture}", null),
                new MenuContainer.MenuItem($"CPU Cores: {SDL.SDL_GetNumLogicalCPUCores()}", null),
                new MenuContainer.MenuItem($"System RAM: {SDL.SDL_GetSystemRAM()}MB", null),
                //new MenuContainer.MenuItem($"GPU Device: {app.GraphicsDevice.Name}", null),
                new MenuContainer.MenuItem($"GPU Driver: {app.GraphicsDevice.Driver}", null),
            }
        });
    }

    private void PushBuildInfoMenu(MenuContainer parent)
    {
        parent.PushSubMenu(new MenuContainer("Build Info")
        {
            Items =
            {
                new MenuContainer.MenuItem($"Game: {app.GameInfo.ApplicationName}", null),
                new MenuContainer.MenuItem($"Version: {app.GameInfo.Version.ToString(3)}", null),
                new MenuContainer.MenuItem(LineSepText, null),
                new MenuContainer.MenuItem($"Stageplay: {GitVersionInformation.SemVer} ({GitVersionInformation.BranchName}.{GitVersionInformation.ShortSha})", null),
                new MenuContainer.MenuItem($"Framework: {RuntimeInformation.FrameworkDescription}", null),
                new MenuContainer.MenuItem($"Foster: {App.FosterVersion.ToString(3)}", null),
                new MenuContainer.MenuItem($"SDL: {GetSdlVersion(SDL.SDL_GetVersion())}", null),
                new MenuContainer.MenuItem($"SDL_mixer: {GetSdlVersion(SDL_mixer.MIX_Version())}", null)
            }
        });
    }

    private static Version GetSdlVersion(int version) => new(((version) / 1000000),
        (((version) / 1000) % 1000), ((version) % 1000));

    private void PushScriptingMenu(MenuContainer parent)
    {
        parent.PushSubMenu(new MenuContainer("LuaScript")
        {
            Items =
            {
                new MenuContainer.MenuItem("TODO", null)
            }
        });
    }

    private void PushScenarioMenu(MenuContainer parent)
    {
        if (app.ActiveScenario is null)
            return;
        
        parent.PushSubMenu(app.ActiveScenario.PushDebugMenu());
    }
}
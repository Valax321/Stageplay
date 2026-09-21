using System.Runtime.CompilerServices;
using Foster.Framework;

namespace Radish.Debugger;

public record struct NavInputs(bool Down, bool Up, bool Select, bool Cancel)
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NavInputs FromInput(Input input)
    {
        return new NavInputs(
            input.Keyboard.PressedOrRepeated(Keys.Down, 0.5f, 0.1f),
            input.Keyboard.PressedOrRepeated(Keys.Up, 0.5f, 0.1f),
            input.Keyboard.Pressed(Keys.Right),
            input.Keyboard.Pressed(Keys.Left)
        );
    }
}
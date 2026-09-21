using System.Runtime.CompilerServices;
using Foster.Framework;

namespace Radish;

internal static class ColorExtensions
{
    extension(Color c)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Color Darken(float scale)
        {
            var (h, s, v) = c.ToHSV();
            v = Math.Clamp(v - scale, 0, 1);
            return Color.FromHSV(h, s, v) with { A = c.A };
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Color Lighten(float scale)
        {
            var (h, s, v) = c.ToHSV();
            v = Math.Clamp(v + scale, 0, 1);
            return Color.FromHSV(h, s, v) with { A = c.A };
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Color ScaleV(float scale)
        {
            var (h, s, v) = c.ToHSV();
            v = Math.Clamp(v * scale, 0, 1);
            return Color.FromHSV(h, s, v) with { A = c.A };
        }
    }
}
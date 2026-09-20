using System.Collections.Concurrent;
using System.Text;

namespace Radish.Utility;

internal static class StringBuilderPool
{
    private static readonly ConcurrentQueue<StringBuilder> _builders = [];
    
    public static StringBuilder Rent()
    {
        if (_builders.TryDequeue(out var sb))
            return sb;

        return new StringBuilder();
    }

    public static void Return(StringBuilder sb)
    {
        sb.Clear();
        _builders.Enqueue(sb);
    }
}
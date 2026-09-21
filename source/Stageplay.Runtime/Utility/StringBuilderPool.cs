using System.Collections.Concurrent;
using System.Text;

namespace Radish.Utility;

internal static class StringBuilderPool
{
    private static readonly ConcurrentQueue<StringBuilder> Builders = [];
    
    public static StringBuilder Rent()
    {
        if (Builders.TryDequeue(out var sb))
            return sb;

        return new StringBuilder();
    }

    public static void Return(StringBuilder sb)
    {
        sb.Clear();
        Builders.Enqueue(sb);
    }
}
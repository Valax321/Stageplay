namespace Radish.ContentBuilder.Utility;

internal static class QueueExtensions
{
    extension<T>(Queue<T> queue)
    {
        public IEnumerable<T> Drain()
        {
            while (queue.TryDequeue(out var o))
                yield return o;
        }
    }
}
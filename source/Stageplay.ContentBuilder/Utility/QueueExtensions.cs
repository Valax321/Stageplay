namespace Radish.ContentBuilder.Utility;

public static class QueueExtensions
{
    extension<T>(Queue<T> queue)
    {
        public IEnumerable<T> ConsumeAll()
        {
            while (queue.TryDequeue(out var o))
                yield return o;
        }
    }
}
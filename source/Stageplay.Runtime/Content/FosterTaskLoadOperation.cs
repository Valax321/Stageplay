using Radish.Resources;

namespace Radish.Content;

internal sealed class FosterTaskLoadOperation<TInterface, TImpl>(Task<TImpl> task, CancellationTokenSource? source = null) : IResourceLoadOperation<TInterface>
    where TInterface : class
    where TImpl : TInterface
{
    public bool IsCompleted => task.IsCompleted;

    public TInterface? Result => task.IsCompleted ? task.Result : null;
    
    public void Cancel()
    {
        source?.Cancel();
    }
}

internal sealed class FosterEmptyLoadOperation<T>(T result) : IResourceLoadOperation<T>
    where T : class
{
    public bool IsCompleted => true;

    public T Result => result;
    
    public void Cancel()
    {
    }
}
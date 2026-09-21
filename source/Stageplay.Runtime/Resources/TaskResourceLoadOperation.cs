namespace Radish.Resources;

internal sealed class TaskResourceLoadOperation<TAsset>(Task<TAsset> task, CancellationTokenSource? source = null) 
    : IResourceLoadOperation<TAsset>
    where TAsset : class
{
    public bool IsCompleted => task.IsCompleted;

    public TAsset? Result => task.IsCompleted ? task.Result : null;
    
    public void Cancel()
    {
        source?.Cancel();
    }
}

internal sealed class SyncResourceLoadOperation<TAsset>(TAsset result) : IResourceLoadOperation<TAsset>
    where TAsset : class
{
    public bool IsCompleted => true;

    public TAsset Result => result;
    
    public void Cancel()
    {
    }
}
using System.Diagnostics.CodeAnalysis;

namespace Radish.Resources;

/// <summary>
/// Wraps an asynchronous asset load request.
/// </summary>
/// <typeparam name="T">The type of resource being loaded.</typeparam>
public interface IResourceLoadOperation<out T> where T : class
{
    /// <summary>
    /// Set to true when the operation completes successfully.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Result))]
    bool IsCompleted { get; }
    
    /// <summary>
    /// The asset that was loaded.
    /// </summary>
    T? Result { get; }

    /// <summary>
    /// Cancels an in-progress load operation.
    /// </summary>
    void Cancel();
}
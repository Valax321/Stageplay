using JetBrains.Annotations;

namespace Radish.ContentBuilder.AssetProcessors;

/// <summary>
/// Describes the inputs and outputs for a single <see cref="AssetProcessor.ProcessContentFile"/> invokation.
/// </summary>
/// <param name="ContentDirectory">The input content directory being processed.</param>
/// <param name="OutputDirectory">The output asset directory being generated.</param>
/// <param name="ContentFilePath">The path relative to <paramref name="ContentDirectory"/> of the source content file being processed.</param>
[PublicAPI]
public record AssetProcessorInput(
    DirectoryInfo ContentDirectory,
    DirectoryInfo OutputDirectory,
    string ContentFilePath
)
{
    /// <summary>
    /// The file descriptor of the source content file.
    /// </summary>
    public FileInfo ContentFile { get; } = new(Path.Combine(ContentDirectory.FullName, ContentFilePath));
}

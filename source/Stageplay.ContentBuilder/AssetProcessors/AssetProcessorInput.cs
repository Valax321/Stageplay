namespace Radish.ContentBuilder.AssetProcessors;

public record AssetProcessorInput(
    DirectoryInfo ContentDirectory,
    DirectoryInfo OutputDirectory,
    string SourceFilePath
)
{
    public FileInfo SourceFile { get; } = new(Path.Combine(ContentDirectory.FullName, SourceFilePath));
}

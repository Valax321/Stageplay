using JetBrains.Annotations;

namespace Radish.ContentBuilder.AssetProcessors;

[PublicAPI]
public abstract class AssetProcessor
{
    public abstract Task<AssetProcessorResult> ProcessContentFile(AssetProcessorInput input);

    protected static void EnsureFileDirectoryExists(string filePath)
    {
        var dirName = Path.GetDirectoryName(filePath);
        if (dirName == null)
            return;

        Directory.CreateDirectory(dirName);
    }
    
    protected static void EnsureFileDirectoryExists(FileInfo file)
    {
        file.Directory?.Create();
    }

    protected static FileInfo MakeOutputFileFromInput(AssetProcessorInput input, string? newExtension = null)
    {
        var f = input.SourceFilePath;
        if (newExtension is not null)
            f = Path.ChangeExtension(f, newExtension);

        return new FileInfo(Path.Combine(input.OutputDirectory.FullName, f));
    }
}

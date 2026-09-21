using JetBrains.Annotations;

namespace Radish.ContentBuilder.AssetProcessors;

/// <summary>
/// Base class for content -> runtime asset format converters.
/// </summary>
[PublicAPI]
public abstract class AssetProcessor
{
    /// <summary>
    /// Converts a content asset into its runtime representation.
    /// </summary>
    /// <param name="input">The input context for the asset being converted.</param>
    /// <returns>Data describing the result of the compilation job.</returns>
    public abstract Task<AssetProcessorResult> ProcessContentFile(AssetProcessorInput input);

    /// <summary>
    /// Takes a file path and creates its containing directory tree if it does not exist.
    /// </summary>
    /// <seealso cref="EnsureFileDirectoryExists(FileInfo)"/>
    /// <param name="filePath">The file path to create a directory tree for.</param>
    protected static void EnsureFileDirectoryExists(string filePath)
    {
        var dirName = Path.GetDirectoryName(filePath);
        if (dirName == null)
            return;

        Directory.CreateDirectory(dirName);
    }
    
    /// <summary>
    /// Takes a file and creates its containing directory tree if it does not exist.
    /// </summary>
    /// <seealso cref="EnsureFileDirectoryExists(string)"/>
    /// <param name="file">The file to create the directory tree for.</param>
    protected static void EnsureFileDirectoryExists(FileInfo file)
    {
        file.Directory?.Create();
    }

    /// <summary>
    /// Takes the input file for an asset processor and returns a corresponding file in the output directory with the same relative path.
    /// </summary>
    /// <param name="input">The asset processor input to create a file from.</param>
    /// <param name="newExtension">If not null, the extension of the output file will be changed to this.</param>
    /// <returns><see cref="FileInfo"/> for the generated output file path.</returns>
    protected static FileInfo MakeOutputFileFromInput(AssetProcessorInput input, string? newExtension = null)
    {
        var f = input.ContentFilePath;
        if (newExtension is not null)
            f = Path.ChangeExtension(f, newExtension);

        return new FileInfo(Path.Combine(input.OutputDirectory.FullName, f));
    }
}

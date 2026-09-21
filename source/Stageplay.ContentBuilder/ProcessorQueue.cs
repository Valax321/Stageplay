using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using Radish.ContentBuilder.AssetProcessors;

namespace Radish.ContentBuilder;

/// <summary>
/// Processes a list of files and executes asset processors for them.
/// </summary>
/// <param name="contentDir">The input content directory to use.</param>
/// <param name="destDir">The output asset directory to create.</param>
public sealed class ProcessorQueue(DirectoryInfo contentDir, DirectoryInfo destDir)
{
    /// <summary>
    /// Describes the final results of a processor queue execution.
    /// </summary>
    /// <param name="TotalProcessed">The total number of asset processor jobs executed.</param>
    /// <param name="Failed">A list of files that failed to be processed and an exception describing the failure.</param>
    public record Result(int TotalProcessed, IReadOnlyCollection<(string, Exception)> Failed);
    
    private readonly Queue<(AssetProcessorInput, AssetProcessor)> _tasks = [];
    
    /// <summary>
    /// Adds multiple files to the queue using a single glob pattern, to be processed by the specified <see cref="AssetProcessor"/>.
    /// </summary>
    /// <seealso cref="AddByGlobPattern(ReadOnlySpan{string}, AssetProcessor)"/>
    /// <param name="pattern">The glob pattern to use to match files in the content directory.</param>
    /// <param name="processor">The processor to process the matched content files with.</param>
    public void AddByGlobPattern(string pattern, AssetProcessor processor)
    {
        var glob = new Matcher();
        glob.AddInclude(pattern);

        foreach (var f in glob.Execute(new DirectoryInfoWrapper(contentDir)).Files)
        {
            _tasks.Enqueue((new AssetProcessorInput(contentDir, destDir, f.Path), processor));
        }
    }
    
    /// <summary>
    /// Adds multiple files to the queue using the given glob patterns, to be processed by the specified <see cref="AssetProcessor"/>.
    /// </summary>
    /// <seealso cref="AddByGlobPattern(string, AssetProcessor)"/>
    /// <param name="patterns">A list of glob patterns to use to match files in the content directory.</param>
    /// <param name="processor">The processor to process the matched content files with.</param>
    public void AddByGlobPattern(ReadOnlySpan<string> patterns, AssetProcessor processor)
    {
        var glob = new Matcher();
        foreach (var p in patterns)
            glob.AddInclude(p);

        foreach (var f in glob.Execute(new DirectoryInfoWrapper(contentDir)).Files)
        {
            _tasks.Enqueue((new AssetProcessorInput(contentDir, destDir, f.Path), processor));
        }
    }

    /// <summary>
    /// Adds a single file to the queue with the specified asset processor.
    /// </summary>
    /// <param name="file">The path to the file to process.</param>
    /// <param name="processor">The processor to process the given file with.</param>
    public void Add(string file, AssetProcessor processor)
    {
        _tasks.Enqueue((new AssetProcessorInput(contentDir, destDir, file), processor));
    }

    internal async Task<Result> ExecuteAll()
    {
        var results = new List<(string, Exception?)>();
        
        foreach (var (i, p) in _tasks.Drain())
        {
            try
            {
                var r = await p.ProcessContentFile(i);
                WriteResult(i, r, null);
                results.Add((i.ContentFilePath, null));
            }
            catch (Exception ex)
            {
                WriteResult(i, null, ex);
                results.Add((i.ContentFilePath, ex));
            }
        }

        return new Result(results.Count, results.Where(x => x.Item2 is not null).ToArray()!);
    }

    private static string GetProperOutputPath(string inputPath, AssetProcessorInput input)
    {
        if (Path.IsPathRooted(inputPath))
            return Path.GetRelativePath(input.OutputDirectory.FullName, inputPath);

        return inputPath;
    }

    private static void WriteResult(AssetProcessorInput input, AssetProcessorResult? output, Exception? error)
    {
        if (output != null)
        {
            Console.Write($"{input.ContentFilePath} -> ");
            Console.Write(output.GeneratedFiles.Count > 1
                ? $"[ {string.Join(", ", output.GeneratedFiles.Select(s => GetProperOutputPath(s, input)))} ]"
                : GetProperOutputPath(output.GeneratedFiles[0], input));
        }
        
        Console.Write(" - ");
        Console.WriteLine(error == null ? "SUCCESS" : $"FAILED ({error.Message})");
    }
}
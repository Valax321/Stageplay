using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using Radish.ContentBuilder.AssetProcessors;

namespace Radish.ContentBuilder;

public class ProcessorQueue(DirectoryInfo contentDir, DirectoryInfo destDir)
{
    public record Result(int TotalProcessed, IReadOnlyCollection<(string, Exception)> Failed);
    
    private readonly Queue<(AssetProcessorInput, AssetProcessor)> _tasks = [];
    
    public void AddByGlobPattern(string pattern, AssetProcessor processor)
    {
        var glob = new Matcher();
        glob.AddInclude(pattern);

        foreach (var f in glob.Execute(new DirectoryInfoWrapper(contentDir)).Files)
        {
            _tasks.Enqueue((new AssetProcessorInput(contentDir, destDir, f.Path), processor));
        }
    }
    
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

    public void Add(string file, AssetProcessor processor)
    {
        _tasks.Enqueue((new AssetProcessorInput(contentDir, destDir, file), processor));
    }

    public async Task<Result> ExecuteAll()
    {
        var results = new List<(string, Exception?)>();
        
        foreach (var (i, p) in _tasks.ConsumeAll())
        {
            try
            {
                var r = await p.ProcessContentFile(i);
                WriteResult(i, r, null);
                results.Add((i.SourceFilePath, null));
            }
            catch (Exception ex)
            {
                WriteResult(i, null, ex);
                results.Add((i.SourceFilePath, ex));
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
            Console.Write($"{input.SourceFilePath} -> ");
            Console.Write(output.GeneratedFiles.Count > 1
                ? $"[ {string.Join(", ", output.GeneratedFiles.Select(s => GetProperOutputPath(s, input)))} ]"
                : GetProperOutputPath(output.GeneratedFiles[0], input));
        }
        
        Console.Write(" - ");
        Console.WriteLine(error == null ? "SUCCESS" : $"FAILED ({error.Message})");
    }
}
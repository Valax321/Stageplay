using CommandLine;
using Radish.ContentBuilder;
using Radish.IO;

namespace Radish;

[Verb("fsarc", HelpText = "Make an fsarc file from the content path.")]
public class MakeFsArcTask : BuilderTask
{
    public override Task<int> RunAsync()
    {
        try
        {
            var file = new FileInfo(OutputPath);
            file.Directory?.Create();
            
            var contentDir = new DirectoryInfo(ContentPath);
            FsArcFile.CreateFromDirectory(file, contentDir);
            return Task.FromResult(0);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Failed to create fsarc:");
            Console.WriteLine(ex.ToString());
            return Task.FromResult(1);
        }
    }
}
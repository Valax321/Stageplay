using JetBrains.Annotations;
using Radish.ContentBuilder.AssetProcessors;

namespace Radish.ContentBuilder.StandardAssetProcessors;

[PublicAPI]
public sealed class AssetCopyProcessor : AssetProcessor
{
    public override Task<AssetProcessorResult> ProcessContentFile(AssetProcessorInput input)
    {
        try
        {
            var outFile = MakeOutputFileFromInput(input);
            input.ContentFile.CopyTo(outFile.FullName, true);

            return Task.FromResult(new AssetProcessorResult([outFile.FullName]));
        }
        catch (Exception exception)
        {
            return Task.FromException<AssetProcessorResult>(exception);
        }
    }
}
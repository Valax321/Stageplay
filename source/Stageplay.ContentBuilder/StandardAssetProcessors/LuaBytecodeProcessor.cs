using JetBrains.Annotations;
using Lua;
using Radish.ContentBuilder.AssetProcessors;
using Radish.Resources;
using Radish.Serialization;

namespace Radish.ContentBuilder.StandardAssetProcessors;

/// <summary>
/// Standard asset processor for converting a Lua script into bytecode for use by the runtime.
/// </summary>
[PublicAPI]
public sealed class LuaBytecodeProcessor : AssetProcessor
{
    /// <inheritdoc/>
    public override async Task<AssetProcessorResult> ProcessContentFile(AssetProcessorInput input)
    {
        using var state = LuaState.Create();
        using var inFile = new StreamReader(input.ContentFile.OpenRead());
        var closure = state.Load(await inFile.ReadToEndAsync(), Path.GetFileNameWithoutExtension(input.ContentFilePath));

        var bc = new LuaBytecodeModule
        {
            Bytecode = closure.Proto.ToBytecode()
        };

        var outFile = MakeOutputFileFromInput(input, "luac");
        EnsureFileDirectoryExists(outFile);

        await using var outStream = outFile.OpenWrite();
        outStream.SetLength(0);

        await BinaryObject.ToStreamAsync(bc, outStream);

        return new AssetProcessorResult([outFile.FullName]);
    }
}
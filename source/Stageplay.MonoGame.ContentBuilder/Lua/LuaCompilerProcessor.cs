using Lua;
using Radish.ContentBuilder.AssetProcessors;
using Radish.MonoGame.Lua;
using Radish.Serialization;

namespace Radish.Lua;

public class LuaCompilerProcessor : AssetProcessor
{
    public override async Task<AssetProcessorResult> ProcessContentFile(AssetProcessorInput input)
    {
        using var state = LuaState.Create();
        using var inFile = new StreamReader(input.SourceFile.OpenRead());
        var closure = state.Load(await inFile.ReadToEndAsync(), Path.GetFileNameWithoutExtension(input.SourceFilePath));

        var bc = new LuaBytecode
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
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Microsoft.Xna.Framework.Content;

namespace Radish.MonoGame.Lua;

/// <summary>
/// XNA type reader for a precompiled lua script.
/// </summary>
[PublicAPI, DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
public sealed class LuaBytecodeReader : ContentTypeReader<LuaBytecode>
{
    /// <inheritdoc/>
    protected override LuaBytecode Read(ContentReader input, LuaBytecode existingInstance)
    {
        var byteCount = input.ReadInt32();
        if (byteCount <= 0)
            throw new InvalidOperationException("Bytecode length must be >= 0");

        return new LuaBytecode(input.ReadBytes(byteCount));
    }
}
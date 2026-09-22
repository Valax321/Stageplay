using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.IO.Pipelines;
using System.Text;
using JetBrains.Annotations;
using MemoryPack;

namespace Radish.Serialization;

/// <summary>
/// Wraps MemoryPack serialization with proper versioning and validation checks.
/// </summary>
[PublicAPI]
public static class BinaryObject
{
    /// <summary>
    /// Deserializes an object from the given stream.
    /// </summary>
    /// <seealso cref="FromStream"/>
    /// <param name="stream">The stream to read from.</param>
    /// <typeparam name="T">The type of object to read.</typeparam>
    /// <returns>The read object.</returns>
    /// <exception cref="BinaryReadException">Thrown if the read object was null.</exception>
    public static async ValueTask<T> FromStreamAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(Stream stream) 
        where T : IBinarySerializable, IMemoryPackable<T>
    {
        ReadPreamble<T>(stream);
        
        var obj = await MemoryPackSerializer.DeserializeAsync<T>(stream);
        return obj ?? throw new BinaryReadException("Object was serialized as null. This should never happen.");
    }
    
    /// <summary>
    /// Deserializes an object from the given stream.
    /// This version operates synchronously.
    /// </summary>
    /// <seealso cref="FromStreamAsync"/>
    /// <param name="stream">The stream to read from.</param>
    /// <typeparam name="T">The type of object to read.</typeparam>
    /// <returns>The read object.</returns>
    /// <exception cref="BinaryReadException">Thrown if the read object was null.</exception>
    public static T FromStream<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(Stream stream) 
        where T : IBinarySerializable, IMemoryPackable<T>
    {
        ReadPreamble<T>(stream);

        var remBufferSize = (int)(stream.Length - stream.Position);
        var mem = ArrayPool<byte>.Shared.Rent(remBufferSize);
        var s = new Span<byte>(mem, 0, remBufferSize);
        stream.ReadExactly(s);
        
        var obj = MemoryPackSerializer.Deserialize<T>(s);
        
        ArrayPool<byte>.Shared.Return(mem);
        return obj ?? throw new BinaryReadException("Object was serialized as null. This should never happen.");
    }

    /// <summary>
    /// Serializes the given object into its binary representation and writes it to the given stream.
    /// </summary>
    /// <seealso cref="ToStream{T}"/>
    /// <param name="obj">The object to write.</param>
    /// <param name="stream">The stream to write to.</param>
    /// <typeparam name="T">The type of object to serialize.</typeparam>
    public static async ValueTask ToStreamAsync<T>(T obj, Stream stream)
        where T : IBinarySerializable, IMemoryPackable<T>
    {
        WritePreamble<T>(stream);
        await MemoryPackSerializer.SerializeAsync(stream, obj);
    }

    
    /// <summary>
    /// Serializes the given object into its binary representation and writes it to the given stream.
    /// This version operates synchronously.
    /// </summary>
    /// <seealso cref="ToStreamAsync{T}"/>
    /// <param name="obj">The object to write.</param>
    /// <param name="stream">The stream to write to.</param>
    /// <typeparam name="T">The type of object to serialize.</typeparam>
    public static void ToStream<T>(T obj, Stream stream)
        where T : IBinarySerializable, IMemoryPackable<T>
    {
        WritePreamble<T>(stream);
        var pw = PipeWriter.Create(stream);
        MemoryPackSerializer.Serialize(pw, obj);
        
        pw.Complete();
    }
    
    private static void ReadPreamble<T>(Stream stream)
        where T : IBinarySerializable
    {
        using var br = new BinaryReader(stream, Encoding.UTF8, true);
        var magic = new FourCC(br.ReadUInt32());
        if (magic != T.HeaderMagic)
            throw new BinaryReadException($"File identifier did not match {T.HeaderMagic}");

        var version = br.ReadUInt32();
        if (version != T.Version)
            throw new BinaryReadException($"File version {version} did not match object format {T.Version}. Content files probably need to be recooked.");
    }
    
    private static void WritePreamble<T>(Stream stream)
        where T : IBinarySerializable
    {
        using var bw = new BinaryWriter(stream, Encoding.UTF8, true);
        bw.Write(T.HeaderMagic.EncodedValue);
        bw.Write(T.Version);
    }
}
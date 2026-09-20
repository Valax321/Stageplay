using MemoryPack;

namespace Radish.Serialization;

/// <summary>
/// An object that can be serialized and deserialized via <see cref="BinaryObject"/>.
/// The class must also implement <see cref="IMemoryPackable{T}"/>.
/// </summary>
public interface IBinarySerializable
{
    /// <summary>
    /// FourCC code for this type.
    /// </summary>
    public static virtual FourCC HeaderMagic => throw new NotImplementedException();
    
    /// <summary>
    /// The current version of this type.
    /// </summary>
    public static virtual uint Version => 1;
}
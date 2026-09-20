using System.Diagnostics;
using System.Globalization;
using MemoryPack;

namespace Radish.Serialization;

/// <summary>
/// A serializable value that can represent a string, integer or boolean.
/// </summary>
[MemoryPackable]
[MemoryPackUnion(0, typeof(StringVariant))]
[MemoryPackUnion(1, typeof(IntVariant))]
[MemoryPackUnion(2, typeof(BoolVariant))]
public partial interface IVariant
{
    /// <summary>
    /// Gets the string representation.
    /// </summary>
    [MemoryPackIgnore]
    string AsString => throw new InvalidOperationException();
    
    /// <summary>
    /// Gets the integer representation.
    /// </summary>
    [MemoryPackIgnore]
    long AsInteger => throw new InvalidOperationException();
    
    /// <summary>
    /// Gets the boolean representation.
    /// </summary>
    [MemoryPackIgnore]
    bool AsBoolean => throw new InvalidOperationException();
}

/// <summary>
/// String variant representation.
/// </summary>
[MemoryPackable]
[DebuggerDisplay("{Value}")]
public partial class StringVariant : IVariant
{
    /// <summary>
    /// The string value.
    /// </summary>
    public required string Value { get; set; }
    
    /// <inheritdoc/>
    public string AsString => Value;

    /// <summary>
    /// Converts a string to a variant.
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static implicit operator StringVariant(string s) => new() { Value = s };
}

/// <summary>
/// Integer variant representation.
/// </summary>
[MemoryPackable]
[DebuggerDisplay("{Value}")]
public partial class IntVariant : IVariant
{
    /// <summary>
    /// The int value.
    /// </summary>
    public long Value { get; set; }
    
    /// <inheritdoc/>
    public string AsString => Value.ToString(CultureInfo.InvariantCulture);
    
    /// <inheritdoc/>
    public long AsInteger => Value;

    /// <summary>
    /// Converts an integer to a variant.
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static implicit operator IntVariant(short s) => new() { Value = s };
    
    /// <summary>
    /// Converts an integer to a variant.
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static implicit operator IntVariant(int s) => new() { Value = s };
    
    /// <summary>
    /// Converts an integer to a variant.
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static implicit operator IntVariant(long s) => new() { Value = s };
}

/// <summary>
/// Boolean variant representation.
/// </summary>
[MemoryPackable]
[DebuggerDisplay("{Value}")]
public partial class BoolVariant : IVariant
{
    /// <summary>
    /// The boolean value.
    /// </summary>
    public bool Value { get; set; }
    
    /// <inheritdoc/>
    public bool AsBoolean => Value;
    
    /// <inheritdoc/>
    public long AsInteger => Value ? 1 : 0;
    
    /// <inheritdoc/>
    public string AsString => Value.ToString(CultureInfo.InvariantCulture);

    /// <summary>
    /// Converts a boolean to a variant.
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static implicit operator BoolVariant(bool s) => new() { Value = s };
}

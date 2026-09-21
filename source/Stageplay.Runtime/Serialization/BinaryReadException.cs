namespace Radish.Serialization;

/// <summary>
/// Exception thrown if an error occurs when deserializing an object via <see cref="BinaryObject"/>.
/// </summary>
/// <param name="message">Human-readable description of what went wrong.</param>
public class BinaryReadException(string message) : Exception(message);

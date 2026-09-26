namespace Radish.Audio;

/// <summary>
/// Thrown when an <see cref="AudioDevice"/> encounters a fatal error.
/// </summary>
/// <param name="message">The message describing the error.</param>
public class AudioException(string message) : Exception(message);

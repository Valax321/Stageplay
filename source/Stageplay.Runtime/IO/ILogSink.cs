using JetBrains.Annotations;

namespace Radish.IO;

/// <summary>
/// Provides the formatted string for a sink log call.
/// </summary>
[PublicAPI]
public interface ISinkWriter
{
    /// <summary>
    /// Gets the message as a span. Does not allocate.
    /// </summary>
    /// <returns></returns>
    public ReadOnlySpan<char> AsSpan();

    /// <summary>
    /// Gets the message as a string. Allocates.
    /// </summary>
    /// <returns></returns>
    public string ToString();
}

/// <summary>
/// Implements a destination for Stageplay's <see cref="Log"/> functions.
/// </summary>
[PublicAPI]
public interface ILogSink
{
    /// <summary>
    /// Writes a log message to the info stream.
    /// </summary>
    /// <param name="message"></param>
    void Info<TWriter>(in TWriter message) where TWriter : ISinkWriter, allows ref struct;
    
    /// <summary>
    /// Writes a log message to the warning stream.
    /// </summary>
    /// <param name="message"></param>
    void Warning<TWriter>(in TWriter message) where TWriter : ISinkWriter, allows ref struct;
    
    /// <summary>
    /// Writes a log message to the error stream.
    /// </summary>
    /// <param name="message"></param>
    void Error<TWriter>(in TWriter message) where TWriter : ISinkWriter, allows ref struct;
}
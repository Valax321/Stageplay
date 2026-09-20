using JetBrains.Annotations;
using Radish.Utility;

namespace Radish;

/// <summary>
/// Static logging methods for Stageplay.
/// </summary>
[PublicAPI]
public static class Log
{
    private readonly ref struct SpanWrapper(ref ReadOnlySpan<char> msg) : ISinkWriter
    {
        private readonly ReadOnlySpan<char> _msg = msg;
        
        public ReadOnlySpan<char> AsSpan()
        {
            return _msg;
        }

        public override string ToString()
        {
            return _msg.ToString();
        }
    }
    
    private static ILogSink? _sink;

    internal static void SetSink(ILogSink sink)
    {
        _sink = sink;
    }
    
    /// <summary>
    /// Logs an information message.
    /// </summary>
    /// <param name="message">The string to log.</param>
    public static void Info(ReadOnlySpan<char> message)
    {
        var s = new SpanWrapper(ref message);
        _sink?.Info(in s);
    }

    /// <summary>
    /// Logs an information message.
    /// </summary>
    /// <param name="message">The string to log.</param>
    public static void Info(ref ZStringInterpolatedStringHandler message)
    {
        _sink?.Info(in message);
        message.Dispose();
    }

    /// <summary>
    /// Logs a warning message.
    /// </summary>
    /// <param name="message">The string to log.</param>
    public static void Warning(ReadOnlySpan<char> message)
    {
        var s = new SpanWrapper(ref message);
        _sink?.Warning(in s);
    }
    
    /// <summary>
    /// Logs an information message.
    /// </summary>
    /// <param name="message">The string to log.</param>
    public static void Warning(ref ZStringInterpolatedStringHandler message)
    {
        _sink?.Warning(in message);
        message.Dispose();
    }

    /// <summary>
    /// Logs an error message.
    /// </summary>
    /// <param name="message">The string to log.</param>
    public static void Error(ReadOnlySpan<char> message)
    {
        var s = new SpanWrapper(ref message);
        _sink?.Error(in s);
    }
    
    /// <summary>
    /// Logs an information message.
    /// </summary>
    /// <param name="message">The string to log.</param>
    public static void Error(ref ZStringInterpolatedStringHandler message)
    {
        _sink?.Error(in message);
        message.Dispose();
    }
}
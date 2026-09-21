namespace Radish.IO;

/// <summary>
/// Writes log messages to the .NET <see cref="Console"/>.
/// </summary>
public sealed class ConsoleLogSink : ILogSink
{
    /// <inheritdoc/>
    public void Info<TWriter>(in TWriter message) 
        where TWriter : ISinkWriter, allows ref struct
    {
        var fg = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("INFO");
        Console.ForegroundColor = fg;
        Console.Write(" : ");
        Console.WriteLine(message.AsSpan());
    }

    /// <inheritdoc/>
    public void Warning<TWriter>(in TWriter message) 
        where TWriter : ISinkWriter, allows ref struct
    {
        var fg = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("WARN");
        Console.ForegroundColor = fg;
        Console.Write(" : ");
        Console.WriteLine(message.AsSpan());
    }

    /// <inheritdoc/>
    public void Error<TWriter>(in TWriter message) 
        where TWriter : ISinkWriter, allows ref struct
    {
        var fg = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Write("ERROR");
        Console.ForegroundColor = fg;
        Console.Write(" : ");
        Console.WriteLine(message.AsSpan());
    }
}
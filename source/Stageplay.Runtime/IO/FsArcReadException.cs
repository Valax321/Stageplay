using System.Numerics;

namespace Radish.IO;

/// <summary>
/// Exception thrown when reading a FSARC file fails.
/// </summary>
/// <param name="file">The file being read.</param>
/// <param name="message">Message describing the failure.</param>
public class FsArcReadException(string file, string message) : Exception($"Error reading {file}: {message}")
{
    internal static void AssertIsPositive<T>(string file, T offset) where T : IBinaryInteger<T>
    {
        if (T.IsNegative(offset))
            throw new FsArcReadException(file, "Value must be a positive number");
    }
}
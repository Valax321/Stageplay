using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Radish.IO;

/// <summary>
/// Parsed key-values from command line options.
/// Regular switches are specified as <c>-switch</c>, and options with a value as specified as <c>-switch=value</c>.
/// </summary>
public sealed class CommandLine : IEnumerable<KeyValuePair<string, string>>
{
    private readonly ImmutableDictionary<string, string> _keyValues;

    internal CommandLine(ReadOnlySpan<string> args)
    {
        var kv = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
        
        foreach (var c in args)
        {
            if (!c.StartsWith('-'))
                continue;

            var b = c[1..];
            var e = b.IndexOf('=');
            if (e < 0)
            {
                kv.Add(b, string.Empty);
            }
            else
            {
                kv.Add(b[..e], b[(e + 1)..]);
            }
        }

        _keyValues = kv.ToImmutableDictionary();
    }
    
    /// <summary>
    /// Checks if the given command line parameter is present.
    /// </summary>
    /// <param name="key">The parameter to check for.</param>
    /// <returns>True if found, otherwise false.</returns>
    public bool Contains(string key)
    {
        return _keyValues.ContainsKey(key);
    }

    /// <summary>
    /// Gets the given command line parameter value.
    /// For switches without a value specified, an empty string will be returned.
    /// </summary>
    /// <param name="key">The parameter to check for.</param>
    /// <param name="value">The parameter's value.</param>
    /// <returns>True if found, otherwise false.</returns>
    public bool TryGetValue(string key, [MaybeNullWhen(false)] out string value)
    {
        return _keyValues.TryGetValue(key, out value);
    }

    /// <inheritdoc/>
    public IEnumerator<KeyValuePair<string, string>> GetEnumerator() 
        => _keyValues.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
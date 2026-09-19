using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Radish;

internal sealed class CommandLine : ICommandLineArguments
{
    private readonly ImmutableDictionary<string, string> _keyValues;

    public CommandLine(ReadOnlySpan<string> args)
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
    
    public bool ContainsKey(string key)
    {
        return _keyValues.ContainsKey(key);
    }

    public bool TryGetValue(string key, [MaybeNullWhen(false)] out string value)
    {
        return _keyValues.TryGetValue(key, out value);
    }

    public IEnumerator<KeyValuePair<string, string>> GetEnumerator() 
        => _keyValues.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() 
        => GetEnumerator();
}
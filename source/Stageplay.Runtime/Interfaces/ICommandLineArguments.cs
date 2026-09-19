using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace Radish;

/// <summary>
/// Interface to a set of command-line parameters parsed as key-value pairs.
/// </summary>
[PublicAPI]
public interface ICommandLineArguments : IEnumerable<KeyValuePair<string, string>>
{
    bool TryGetValue(string key, [MaybeNullWhen(false)] out string value);
    bool ContainsKey(string key);
}

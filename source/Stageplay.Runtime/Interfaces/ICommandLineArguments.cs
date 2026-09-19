using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace Radish;

/// <summary>
/// Interface to a set of command-line parameters parsed as key-value pairs.
/// Key lookup is done as <see cref="StringComparer.InvariantCultureIgnoreCase"/>.
/// </summary>
[PublicAPI]
public interface ICommandLineArguments : IEnumerable<KeyValuePair<string, string>>
{
    /// <summary>
    /// Gets the command line argument value from its key if it exists.
    /// If an option is specified without a value (as -key instead of -key=value) the value will be an empty string.
    /// </summary>
    /// <param name="key">The key to look for.</param>
    /// <param name="value">The value of the command line option.</param>
    /// <returns>True of the key was found, otherwise false.</returns>
    bool TryGetValue(string key, [MaybeNullWhen(false)] out string value);
    
    /// <summary>
    /// Gets whether the given command line argument was present at all.
    /// </summary>
    /// <param name="key">The key to check for.</param>
    /// <returns>True if present, otherwise false.</returns>
    bool ContainsKey(string key);
}

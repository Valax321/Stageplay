using Radish.Scenario;

namespace Radish.ContentBuilder.Scenario;

/// <summary>
/// Build time representation of a scenario string table.
/// </summary>
public sealed class WritableStringTable
{
    private readonly Dictionary<string, int> _lookup = new(StringComparer.InvariantCulture);
    
    /// <summary>
    /// Gets a unique integer that represents the string.
    /// If the string isn't present in the table, it will be added and a new id generated.
    /// </summary>
    /// <param name="value">The string to add.</param>
    /// <returns>The string's unique ID.</returns>
    public int GetUniqueStringIndex(string value)
    {
        if (_lookup.TryGetValue(value, out var index))
            return index;
        
        index = _lookup.Count;
        _lookup.Add(value, index);
        return index;
    }

    /// <summary>
    /// Creates the runtime representation of this string table.
    /// </summary>
    /// <returns>The generated string table.</returns>
    public ScenarioStringTable BuildRuntimeStringTable()
    {
        var strings = new string[_lookup.Count];
        foreach (var (v, index) in _lookup)
        {
            strings[index] = v;
        }

        return new ScenarioStringTable
        {
            Strings = strings
        };
    }
}
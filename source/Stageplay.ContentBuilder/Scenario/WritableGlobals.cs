using System.Collections.Immutable;
using Radish.Scenario;

namespace Radish.ContentBuilder.Scenario;

/// <summary>
/// Build-time scenario globals table.
/// </summary>
/// <param name="stringTable">The string table to get string indices from.</param>
public class WritableGlobals(WritableStringTable stringTable)
{
    private readonly Dictionary<int, int> _lookup = [];

    /// <summary>
    /// Adds a new global definition.
    /// </summary>
    /// <param name="name">The name of the variable.</param>
    /// <param name="defaultValue">The initial value of the variable.</param>
    /// <returns>True if it was added, false if a variable already existed with this name.</returns>
    public bool Add(string name, int defaultValue)
    {
        var idx = stringTable.GetUniqueStringIndex(name);
        return _lookup.TryAdd(idx, defaultValue);
    }

    /// <summary>
    /// Creates the runtime representation of this globals table.
    /// </summary>
    /// <returns></returns>
    public ScenarioGlobals BuildRuntimeGlobals() => new()
    {
        State = _lookup.ToImmutableDictionary()
    };
}
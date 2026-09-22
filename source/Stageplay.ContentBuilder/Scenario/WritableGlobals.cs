using System.Collections.Immutable;
using Radish.Scenario;

namespace Radish.ContentBuilder.Scenario;

public class WritableGlobals(WritableStringTable stringTable)
{
    private Dictionary<int, int> _lookup = [];

    public bool Add(string name, int defaultValue)
    {
        var idx = stringTable.GetUniqueStringIndex(name);
        return _lookup.TryAdd(idx, defaultValue);
    }

    public ScenarioGlobals BuildRuntimeGlobals() => new()
    {
        State = _lookup.ToImmutableDictionary()
    };
}
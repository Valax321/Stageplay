using Radish.Scenario;

namespace Radish.ContentBuilder.Scenario;

public sealed class WritableStringTable
{
    private readonly Dictionary<string, int> _lookup = new(StringComparer.InvariantCulture);
    
    public int GetUniqueStringIndex(string value)
    {
        if (_lookup.TryGetValue(value, out var index))
            return index;

        index = _lookup.Count;
        _lookup.Add(value, index);
        return index;
    }

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
using JetBrains.Annotations;

namespace Radish.Scenario;

[PublicAPI]
public interface IScenarioGlobalState
{
    bool Exists(string varName);

    int this[string name]
    {
        get;
        set;
    }
}
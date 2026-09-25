using JetBrains.Annotations;

namespace Radish.Scenario.Commands;

[PublicAPI]
public interface ICommandTokens
{
    string Command { get; }
    
    int Count { get; }
    
    string AsString(int index);
    int AsInt(int index);
    float AsFloat(int index);

    bool IsKeyword(string keyword, int index);
}
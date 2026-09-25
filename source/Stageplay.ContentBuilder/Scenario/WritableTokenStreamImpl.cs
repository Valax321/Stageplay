using Radish.Scenario.Commands;

namespace Radish.ContentBuilder.Scenario;

internal sealed class WritableTokenStreamImpl(IReadOnlyList<string> tokens, string file, int line) : ICommandTokens
{
    public string Command => tokens[0];
    public int Count => tokens.Count - 1;
    
    public string AsString(int index)
    {
        if (index + 1 > Count)
            throw new ScenarioParseException("SCR0100", file, line, "too few arguments in command call");
        
        return tokens[index + 1];
    }

    public int AsInt(int index)
    {
        if (index + 1 > Count)
            throw new ScenarioParseException("SCR0100", file, line, "too few arguments in command call");
        
        return int.Parse(tokens[index + 1]);
    }

    public float AsFloat(int index)
    {
        if (index + 1 > Count)
            throw new ScenarioParseException("SCR0100", file, line, "too few arguments in command call");
        
        return float.Parse(tokens[index + 1]);
    }

    public bool AsBoolean(int index)
    {
        if (index + 1 > Count)
            throw new ScenarioParseException("SCR0100", file, line, "too few arguments in command call");
        
        if (byte.TryParse(tokens[index + 1], out var b))
            return b > 0;

        return bool.Parse(tokens[index + 1]);
    }

    public bool IsKeyword(string keyword, int index)
    {
        return tokens[index + 1].Equals(keyword, StringComparison.InvariantCultureIgnoreCase);
    }
}
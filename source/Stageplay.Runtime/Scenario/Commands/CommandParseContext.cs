namespace Radish.Scenario.Commands;

public readonly record struct CommandParseContext(ICommandTokens Tokens, (string File, int Line) SourceLocation)
{
    public void RegisterAssetDependency(string assetPath)
    {
        
    }
}
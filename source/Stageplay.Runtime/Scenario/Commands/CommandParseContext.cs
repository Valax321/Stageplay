namespace Radish.Scenario.Commands;

public readonly record struct CommandParseContext(
    ICommandTokens Tokens,
    (string File, int Line) SourceLocation,
    IWritableStringTable StringTable,
    IReadOnlyList<CommandArgumentType> ExpectedArguments)
{
    public void RegisterAssetDependency(string assetPath)
    {
    }

    public int GetStringIndex(string val)
    {
        return StringTable.GetUniqueStringIndex(val);
    }
}
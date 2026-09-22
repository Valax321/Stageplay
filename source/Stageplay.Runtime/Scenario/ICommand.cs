namespace Radish.Scenario;

public interface ICommandTokens
{
    string Command { get; }
    
    int Count { get; }
    
    string AsString(int index);
    int AsInt(int index);
    float AsFloat(int index);

    bool IsKeyword(string keyword, int index);
}

public readonly record struct CommandData(string Name, params IReadOnlyList<object> Arguments);

public readonly record struct CommandExecutionContext(BytecodeReader Reader, ScenarioVM Vm)
{
    public void Complete()
    {
        Vm.MoveToNextCommand();
    }

    /// <summary>
    /// Continues executing this command latently, using the given parameter packet.
    /// To avoid allocations, <paramref name="func"/> MUST be a static method!
    /// </summary>
    /// <param name="value">The parameter packet to pass to the function.</param>
    /// <param name="func">The latent per-frame method. Should be a static method.</param>
    /// <typeparam name="T">The parameter packet type.</typeparam>
    public void ContinueWith<T>(in T value, Action<CommandContinuationContext<T>> func) where T : struct
    {
        
    }
}

public readonly record struct CommandContinuationContext<T>(T Params, ScenarioVM Vm)
{
    public void Complete()
    {
        Vm.MoveToNextCommand();
    }
}

public readonly record struct CommandParseContext(ICommandTokens Tokens)
{
    public void RegisterAssetDependency(string assetPath)
    {
        
    }
}

public interface ICommand
{
    void BeginExecute(in CommandExecutionContext ctx);
    CommandData? Parse(in CommandParseContext ctx);
}
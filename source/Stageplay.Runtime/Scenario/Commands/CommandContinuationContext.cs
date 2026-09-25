namespace Radish.Scenario.Commands;

public readonly record struct CommandContinuationContext<T>(T Params, ScenarioVM Vm)
{
    public CommandReturn Complete() => CommandReturn.Complete;
    
    public CommandReturn Halt() => CommandReturn.Halt;
}
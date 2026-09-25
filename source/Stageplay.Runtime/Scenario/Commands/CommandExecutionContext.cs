namespace Radish.Scenario.Commands;

public readonly record struct CommandExecutionContext(BytecodeReader Reader, ScenarioVM Vm, int ArgumentCount)
{
    public CommandReturn Complete() => CommandReturn.Complete;

    public CommandReturn Halt() => CommandReturn.Halt;

    /// <summary>
    /// Continues executing this command latently, using the given parameter packet.
    /// To avoid allocations, <paramref name="func"/> MUST be a static method!
    /// </summary>
    /// <param name="value">The parameter packet to pass to the function.</param>
    /// <param name="func">The latent per-frame method. Should be a static method.</param>
    /// <typeparam name="T">The parameter packet type.</typeparam>
    public CommandReturn ContinueWith<T>(T value, Func<CommandContinuationContext<T>, CommandReturn> func) 
        where T : struct
    {
        //FIXME: this allocates anyway
        var me = this;
        Vm.SetContinuationAction(() => func(new CommandContinuationContext<T>(value, me.Vm)));
        return CommandReturn.Continue;
    }
}
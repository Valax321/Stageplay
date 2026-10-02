using JetBrains.Annotations;

namespace Radish.Scenario.Commands;

[PublicAPI]
public readonly ref struct CommandExecutionContext
{
    public ScenarioVM Vm => _vm;
    
    private readonly ScenarioVM _vm;
    private readonly ReadOnlySpan<ArgumentValue> _arguments;

    internal CommandExecutionContext(ScenarioVM vm, ReadOnlySpan<ArgumentValue> arguments)
    {
        _vm = vm;
        _arguments = arguments;
    }

    public CommandReturn Complete() => CommandReturn.Complete;

    public CommandReturn Halt() => CommandReturn.Halt;

    public byte GetArgumentAsByte(int index)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, _arguments.Length);
        return _arguments[index].AsByte;
    }
    
    public int GetArgumentAsInt(int index)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, _arguments.Length);
        return _arguments[index].AsInteger;
    }
    
    public float GetArgumentAsFloat(int index)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, _arguments.Length);
        return _arguments[index].AsFloat;
    }
    
    public bool GetArgumentAsBool(int index)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, _arguments.Length);
        return _arguments[index].AsBoolean;
    }

    public string GetArgumentAsString(int index)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, _arguments.Length);
        return _vm.Script.GetStringByIndex(_arguments[index].AsStringIndex);
    }

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
        var vm = _vm;
        _vm.SetContinuationAction(() => func(new CommandContinuationContext<T>(value, vm)));
        return CommandReturn.Continue;
    }
}
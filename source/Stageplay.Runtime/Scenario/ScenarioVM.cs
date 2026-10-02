using System.Buffers;
using System.Collections.Immutable;
using System.Reflection;
using JetBrains.Annotations;
using Radish.Debugger;
using Radish.Resources;
using Radish.Scenario.Commands;

namespace Radish.Scenario;

[PublicAPI]
public sealed class ScenarioVM
{
    public string Name { get; }
    public StageplayRuntime Owner { get; }
    public CompiledScenario Script { get; }
    public double CurrentTime => Owner.Time.Seconds;
    public bool FastForward { get; private set; }

    public int InstructionPointer
    {
        get => _reader.Position;
        [Obsolete("Use SetInstructionPointer as it does bounds checking.")]
        private set => _reader.Position = value;
    }

    private readonly Stack<int> _instructionStack = [];
    private Func<CommandReturn>? _continueAction;
    private bool _haltVm;
    private readonly BytecodeReader _reader;
    private ImmutableDictionary<string, ICommand> _commands;
    private SourceLocation _location;

    internal ScenarioVM(StageplayRuntime app, string name, CompiledScenario script, IReadOnlyDictionary<string, ICommand> commands)
    {
        Name = name;
        Owner = app;
        Script = script;
        _reader = new BytecodeReader(Script.Bytecode.Data, Script.StringTable);
        _commands = commands.ToImmutableDictionary();

        var startAddress = Script.FindLabelAddressByName(Script.StartLabel);
        if (!startAddress.HasValue)
            throw new ScenarioExecutionException($"Could not find start label address \"{Script.StartLabel}\"");
        
        SetInstructionPointer(startAddress.Value);
    }

    internal void Update()
    {
        if (_haltVm)
            return;

        RunCommandFrameLoop();
    }

    public void Halt()
    {
        Log.Info($"VM halted at {GetSourceLocationString(_location)}");
        _haltVm = true;
    }

    public void SetInstructionPointer(int address)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(address);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(address, Script.Bytecode.Data.Length);
        
#pragma warning disable CS0618 // Type or member is obsolete
        InstructionPointer = address;
#pragma warning restore CS0618 // Type or member is obsolete
    }

    public void PushInstructionPointer(int address)
    {
        _instructionStack.Push(InstructionPointer);
        SetInstructionPointer(address);
    }

    public void PopInstructionPointer()
    {
        SetInstructionPointer(_instructionStack.Pop());
    }

    public void EndScenario()
    {
        Log.Info($"End of scenario");
    }

    public string GetSourceLocationString(SourceLocation location)
    {
        return $"{Script.GetStringByIndex(location.FileStringIndex)}:{location.Line}";
    }
    
    #region Command Loop

    private enum KeepExecuting
    {
        No,
        Yes
    }
    
    private void RunCommandFrameLoop()
    {
        while (!_haltVm)
        {
            if (_continueAction is not null)
            {
                var result = _continueAction();
                if (HandleCommandResult(result) == KeepExecuting.No)
                    break;
            }

            if (ExecuteNextCommand() == KeepExecuting.No)
                break;
        }
    }
    
    private KeepExecuting HandleCommandResult(CommandReturn result)
    {
        switch (result)
        {
            case CommandReturn.Complete:
                _continueAction = null; // clear the current frame function
                return KeepExecuting.Yes; // Keep running functions this frame
            
            case CommandReturn.Continue:
                return KeepExecuting.No; // Done for this frame
            
            case CommandReturn.Halt:
                Halt();
                return KeepExecuting.No; // Don't keep going after a halt
            
            default:
                throw new ArgumentOutOfRangeException(nameof(result), result, null);
        }
    }

    private KeepExecuting ExecuteNextCommand()
    {
        var location = Script.GetSourceLocation(InstructionPointer);
        if (location is null)
        {
            throw new InvalidOperationException(
                $"Got invalid source location from {GetSourceLocationString(_location)} (IP: 0x{InstructionPointer:x8})");
        }
        _location = location.Value;

        var commandName = _reader.ReadString();
        
        if (!_commands.TryGetValue(commandName, out var cmd))
        {
            Log.Error($"Unknown command named \"{commandName}\" at {GetSourceLocationString(_location)}");
            Halt();
            return KeepExecuting.No;
        }

        var cmdArgsInfo = cmd.GetType().GetCustomAttribute<CommandArgumentsAttribute>();
        if (cmdArgsInfo is null)
            throw new InvalidOperationException(
                $"{GetSourceLocationString(_location)}: command {cmd.GetType().FullName} must have {nameof(CommandArgumentsAttribute)}");
        
        var args = ArrayPool<ArgumentValue>.Shared.Rent(cmdArgsInfo.Arguments.Count);
        for (var i = 0; i < cmdArgsInfo.Arguments.Count; ++i)
        {
            args[i] = cmdArgsInfo.Arguments[i] switch
            {
                CommandArgumentType.Byte => ArgumentValue.OfByte(_reader.ReadByte()),
                CommandArgumentType.Integer => ArgumentValue.OfInteger(_reader.ReadInteger()),
                CommandArgumentType.Float => ArgumentValue.OfFloat(_reader.ReadFloat()),
                CommandArgumentType.String => ArgumentValue.OfString(_reader.ReadStringIndex()),
                CommandArgumentType.Boolean => ArgumentValue.OfBoolean(_reader.ReadBoolean()),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        
        var ctx = new CommandExecutionContext(this, new ReadOnlySpan<ArgumentValue>(args, 0, cmdArgsInfo.Arguments.Count));
        var result = cmd.BeginExecute(in ctx);
        
        ArrayPool<ArgumentValue>.Shared.Return(args);
        return HandleCommandResult(result);
    }

    internal void SetContinuationAction(Func<CommandReturn> action)
    {
        _continueAction = action;
    }
    
    #endregion

    #region Debug Menu

    internal MenuContainer PushDebugMenu()
    {
        return new MenuContainer("Scenario")
        {
            Items = 
            [
                new MenuContainer.MenuItem($"Current Scenario: {Name}", null),
                new MenuContainer.MenuItem($"Stacktrace: {GetSourceLocationString(_location)}", null),
                new MenuContainer.MenuItem($"IP: 0x{InstructionPointer:X8}", null),
                new MenuContainer.MenuItem($"Halted: {_haltVm}", null)
            ]
        };
    }

    #endregion
}
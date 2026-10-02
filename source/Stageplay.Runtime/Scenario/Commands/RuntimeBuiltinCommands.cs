using System.Buffers;
using Cysharp.Text;
using Foster.Framework;
using Radish.Utility;

namespace Radish.Scenario.Commands;

/// <summary>
/// Table of builtin scenario commands.
/// </summary>
public static class RuntimeBuiltinCommands
{
    /// <summary>
    /// Default command table.
    /// </summary>
    public static IReadOnlyDictionary<string, ICommand> Table { get; } = new Dictionary<string, ICommand>
    {
        { "dprint", new DebugPrint() },
        { "msg", new ShowMessage(false) },
        { "append", new ShowMessage(true) },
        { "end", new EndScenario() },
        { "wait", new WaitForSeconds() },
        { "waitkey", new WaitForKeypress() },
        { "fadeto", new FadeTo() },
        { "justify", new TextJustify() },
        { "pos", new TextPosition() },
        { "size", new TextSize() },
        { "goto", new GoToLabel() },
        { "jump", new GoToLabel() },
        { "bg", new SetBackground() },
        { "playsound", new PlaySoundEffect() },
        { "stopsound", new StopSoundEffect() },
        { "pushmenu", new PushMenu() },
    };

    [CommandArguments("s")]
    private sealed class ShowMessage(bool append) : ICommand
    {
        public CommandReturn BeginExecute(in CommandExecutionContext ctx)
        {
            return ctx.Complete();
        }
    }

    [CommandArguments("if")]
    private sealed class FadeTo : ICommand
    {
        public CommandReturn BeginExecute(in CommandExecutionContext ctx)
        {
            return ctx.Complete();
        }

        public CommandData? Parse(in CommandParseContext ctx)
        {
            var colorStr = ctx.Tokens.AsString(0);
            Color c;
            if (colorStr.StartsWith('#'))
                c = Color.FromHexStringRGBA(colorStr);
            else
            {
                c = colorStr switch
                {
                    "black" => Color.Black,
                    "white" => Color.White,
                    "transparent" => Color.Transparent,
                    _ => throw new ArgumentOutOfRangeException(null, "Unknown color name")
                };
            }

            var time = ctx.Tokens.AsFloat(1);
            return new CommandData(ctx.Tokens.Command, c.RGBA, time);
        }
    }

    [CommandArguments("s")]
    private sealed class SetBackground : ICommand
    {
        public CommandReturn BeginExecute(in CommandExecutionContext ctx)
        {
            var bgName = ctx.GetArgumentAsString(0);
            return ctx.Complete();
        }
    }

    [CommandArguments("u")]
    private sealed class TextJustify : ICommand
    {
        public CommandReturn BeginExecute(in CommandExecutionContext ctx)
        {
            return ctx.Complete();
        }

        public CommandData? Parse(in CommandParseContext ctx)
        {
            var justifyValue = (byte)Enum.Parse<Graphics.TextJustify>(ctx.Tokens.AsString(0), true);
            return new CommandData(ctx.Tokens.Command, justifyValue);
        }
    }

    [CommandArguments("ff")]
    private sealed class TextPosition : ICommand
    {
        public CommandReturn BeginExecute(in CommandExecutionContext ctx)
        {
            return ctx.Complete();
        }
    }

    [CommandArguments("i")]
    private sealed class TextSize : ICommand
    {
        public CommandReturn BeginExecute(in CommandExecutionContext ctx)
        {
            return ctx.Complete();
        }
    }

    [CommandArguments("si")]
    private sealed class PlaySoundEffect : ICommand
    {
        public CommandReturn BeginExecute(in CommandExecutionContext ctx)
        {
            return ctx.Complete();
        }

        public CommandData? Parse(in CommandParseContext ctx)
        {
            var soundName = ctx.Tokens.AsString(0);
            ctx.RegisterAssetDependency(soundName);

            var soundChannel = -1;
            if (ctx.Tokens.Count > 1)
                soundChannel = ctx.Tokens.AsInt(1);

            return new CommandData(ctx.Tokens.Command,
                ArgumentValue.OfString(ctx.StringTable.GetUniqueStringIndex(soundName)), soundChannel);
        }
    }

    [CommandArguments("i")]
    private sealed class StopSoundEffect : ICommand
    {
        public CommandReturn BeginExecute(in CommandExecutionContext ctx)
        {
            return ctx.Complete();
        }

        public CommandData? Parse(in CommandParseContext ctx)
        {
            var soundChannel = -1;
            if (ctx.Tokens.Count > 0)
                soundChannel = ctx.Tokens.AsInt(0);

            return new CommandData(ctx.Tokens.Command, soundChannel);
        }
    }

    [CommandArguments("fb")]
    private sealed class WaitForSeconds : ICommand
    {
        private readonly record struct Params(double FinishTime, bool AllowSkip);

        public CommandReturn BeginExecute(in CommandExecutionContext ctx)
        {
            var t = ctx.GetArgumentAsFloat(0);
            var allowSkip = ctx.GetArgumentAsBool(1);
            var p = new Params(ctx.Vm.CurrentTime + t, allowSkip);
            return ctx.ContinueWith(p, Frame);
        }

        private static CommandReturn Frame(CommandContinuationContext<Params> ctx)
        {
            if (ctx.Vm.FastForward)
                return ctx.Complete();

            if (ctx.Params.FinishTime >= ctx.Vm.CurrentTime)
                return ctx.Complete();

            return CommandReturn.Continue;
        }

        public CommandData? Parse(in CommandParseContext ctx)
        {
            var waitFor = ctx.Tokens.AsFloat(0);
            var allowSkip = false;
            if (ctx.Tokens.Count > 1)
                allowSkip = ctx.Tokens.IsKeyword("allowskip", 1);

            return new CommandData(ctx.Tokens.Command, waitFor, allowSkip);
        }
    }

    [CommandArguments("")]
    private sealed class WaitForKeypress : ICommand
    {
        public CommandReturn BeginExecute(in CommandExecutionContext ctx)
        {
            return ctx.ContinueWith(new Unit(), Frame);
        }

        private static CommandReturn Frame(CommandContinuationContext<Unit> ctx)
        {
            if (ctx.Vm.FastForward)
                return ctx.Complete();

            return CommandReturn.Continue;
        }
    }

    [CommandArguments("s")]
    private sealed class DebugPrint : ICommand
    {
        public CommandReturn BeginExecute(in CommandExecutionContext ctx)
        {
            Log.Info(ctx.GetArgumentAsString(0));
            return ctx.Complete();
        }
    }

    [CommandArguments("s")]
    private sealed class GoToLabel : ICommand
    {
        public CommandReturn BeginExecute(in CommandExecutionContext ctx)
        {
            var labelString = ctx.GetArgumentAsString(0);
            var address = ctx.Vm.Script.FindLabelAddressByName(labelString);
            if (address is null)
            {
                Log.Error($"Failed to get address for label \"{labelString}\"");
                return ctx.Halt();
            }

            ctx.Vm.SetInstructionPointer(address.Value);
            return ctx.Complete();
        }
    }

    [CommandArguments("")]
    private sealed class EndScenario : ICommand
    {
        public CommandReturn BeginExecute(in CommandExecutionContext ctx)
        {
            ctx.Vm.EndScenario();
            return ctx.Halt();
        }
    }

    [CommandArguments("s")]
    private sealed class PushMenu : ICommand
    {
        public CommandReturn BeginExecute(in CommandExecutionContext ctx)
        {
            var menuScriptName = ctx.GetArgumentAsString(0);
            Log.Info($"PushMenu script {menuScriptName}");
            return ctx.Complete();
        }

        public CommandData? Parse(in CommandParseContext ctx)
        {
            var menuName = ctx.Tokens.AsString(0);
            return new CommandData(ctx.Tokens.Command, ArgumentValue.OfString(ctx.StringTable.GetUniqueStringIndex(menuName)));
        }
    }
}
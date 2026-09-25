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
        {"dprint", new DebugPrint()},
        {"msg", new ShowMessage(false)},
        {"append", new ShowMessage(true)},
        {"end", new EndScenario()},
        {"wait", new WaitForSeconds()},
        {"waitkey", new WaitForKeypress()},
        {"fadeto", new FadeTo()},
        {"justify", new TextJustify()},
        {"pos", new TextPosition()},
        {"size", new TextSize()},
        {"goto", new GoToLabel()},
        {"jump", new GoToLabel()},
        {"bg", new SetBackground()},
        {"playsound", new PlaySoundEffect()},
        {"stopsound", new StopSoundEffect()},
        {"pushmenu", new PushMenu()},
    };

    private sealed class ShowMessage(bool append) : ICommand
    {
        public CommandReturn BeginExecute(in CommandExecutionContext ctx)
        {
            return ctx.Complete();
        }

        public CommandData? Parse(in CommandParseContext ctx)
        {
            return new CommandData(ctx.Tokens.Command, ctx.Tokens.AsString(0));
        }
    }

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
            return new CommandData(ctx.Tokens.Command, c.R, c.G, c.B, c.A, time);
        }
    }

    private sealed class SetBackground : ICommand
    {
        public CommandReturn BeginExecute(in CommandExecutionContext ctx)
        {
            var bgName = ctx.Reader.ReadString();
            return ctx.Complete();
        }

        public CommandData? Parse(in CommandParseContext ctx)
        {
            var bgName = $"images/{ctx.Tokens.AsString(0)}";
            ctx.RegisterAssetDependency(bgName);
            return new CommandData(ctx.Tokens.Command, bgName);
        }
    }

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

    private sealed class TextPosition : ICommand
    {
        public CommandReturn BeginExecute(in CommandExecutionContext ctx)
        {
            return ctx.Complete();
        }

        public CommandData? Parse(in CommandParseContext ctx)
        {
            var x = ctx.Tokens.AsFloat(0);
            var y = ctx.Tokens.AsFloat(1);

            return new CommandData(ctx.Tokens.Command, x, y);
        }
    }

    private sealed class TextSize : ICommand
    {
        public CommandReturn BeginExecute(in CommandExecutionContext ctx)
        {
            return ctx.Complete();
        }

        public CommandData? Parse(in CommandParseContext ctx)
        {
            var sz = ctx.Tokens.AsInt(0);
            return new CommandData(ctx.Tokens.Command, sz);
        }
    }

    private sealed class PlaySoundEffect : ICommand
    {
        public CommandReturn BeginExecute(in CommandExecutionContext ctx)
        {
            return ctx.Complete();
        }

        public CommandData? Parse(in CommandParseContext ctx)
        {
            var soundName = $"sound/{ctx.Tokens.AsString(0)}";
            ctx.RegisterAssetDependency(soundName);

            var soundChannel = -1;
            if (ctx.Tokens.Count > 1)
                soundChannel = ctx.Tokens.AsInt(1);
            
            return new CommandData(ctx.Tokens.Command, soundName, soundChannel);
        }
    }
    
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

    private sealed class WaitForSeconds : ICommand
    {
        private readonly record struct Params(double FinishTime, bool AllowSkip);
        
        public CommandReturn BeginExecute(in CommandExecutionContext ctx)
        {
            var p = new Params(ctx.Vm.CurrentTime + ctx.Reader.ReadFloat(), ctx.Reader.ReadBoolean());
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

        public CommandData? Parse(in CommandParseContext ctx)
        {
            return new CommandData(ctx.Tokens.Command);
        }
    }

    private sealed class DebugPrint : ICommand
    {
        public CommandReturn BeginExecute(in CommandExecutionContext ctx)
        {
            var items = ArrayPool<string>.Shared.Rent(ctx.ArgumentCount);
            
            for (var i = 0; i < ctx.ArgumentCount; ++i)
                items[i] = ctx.Reader.ReadString();
            
            using var sb = ZString.CreateStringBuilder();
            sb.AppendJoin(' ', items);
            
            ArrayPool<string>.Shared.Return(items);
            
            Log.Info(sb.AsSpan());
            return ctx.Complete();
        }

        public CommandData? Parse(in CommandParseContext ctx)
        {
            var tokens = new List<object>();
            for (var i = 0; i < ctx.Tokens.Count; ++i)
            {
                tokens.Add(ctx.Tokens.AsString(i));
            }
            
            return new CommandData(ctx.Tokens.Command, tokens);
        }
    }

    private sealed class GoToLabel : ICommand
    {
        public CommandReturn BeginExecute(in CommandExecutionContext ctx)
        {
            var labelString = ctx.Reader.ReadString();
            var address = ctx.Vm.Script.FindLabelAddressByName(labelString);
            if (address is null)
            {
                Log.Error($"Failed to get address for label \"{labelString}\"");
                return ctx.Halt();
            }
            
            ctx.Vm.SetInstructionPointer(address.Value);
            return ctx.Complete();
        }

        public CommandData? Parse(in CommandParseContext ctx)
        {
            var label = ctx.Tokens.AsString(0);
            return new CommandData(ctx.Tokens.Command, label);
        }
    }
    
    private sealed class EndScenario : ICommand
    {
        public CommandReturn BeginExecute(in CommandExecutionContext ctx)
        {
            ctx.Vm.EndScenario();
            return ctx.Halt();
        }

        public CommandData? Parse(in CommandParseContext ctx)
        {
            return new CommandData(ctx.Tokens.Command);
        }
    }

    private sealed class PushMenu : ICommand
    {
        public CommandReturn BeginExecute(in CommandExecutionContext ctx)
        {
            var menuScriptName = ctx.Reader.ReadString();
            Log.Info($"PushMenu script {menuScriptName}");
            return ctx.Complete();
        }

        public CommandData? Parse(in CommandParseContext ctx)
        {
            var menuName = ctx.Tokens.AsString(0);
            return new CommandData(ctx.Tokens.Command, menuName);
        }
    }
}
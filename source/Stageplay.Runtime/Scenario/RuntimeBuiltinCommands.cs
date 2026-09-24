using Foster.Framework;

namespace Radish.Scenario;

public static class RuntimeBuiltinCommands
{
    public static IReadOnlyDictionary<string, ICommand> Table = new Dictionary<string, ICommand>
    {
        {"debugprint", new DebugPrint()},
        {"msg", new ShowMessage(false)},
        {"append", new ShowMessage(true)},
        {"end", new EndScenario()},
        {"wait", new WaitForSeconds()},
        {"waitkey", new WaitForKeypress()},
        {"fadeto", new FadeTo()},
        {"justify", new TextJustify()},
        {"pos", new TextPosition()},
        {"size", new TextSize()},
        {"jump", new JumpToLabel()},
        {"bg", new SetBackground()},
        {"playsound", new PlaySoundEffect()},
        {"stopsound", new StopSoundEffect()}
    };

    private sealed class ShowMessage(bool append) : ICommand
    {
        public void BeginExecute(in CommandExecutionContext ctx)
        {
            
        }

        public CommandData? Parse(in CommandParseContext ctx)
        {
            return new CommandData(ctx.Tokens.Command, ctx.Tokens.AsString(0));
        }
    }

    private sealed class FadeTo : ICommand
    {
        public void BeginExecute(in CommandExecutionContext ctx)
        {
            
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
        public void BeginExecute(in CommandExecutionContext ctx)
        {
            
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
        public void BeginExecute(in CommandExecutionContext ctx)
        {
            
        }

        public CommandData? Parse(in CommandParseContext ctx)
        {
            var justifyValue = (byte)Enum.Parse<Graphics.TextJustify>(ctx.Tokens.AsString(0), true);
            return new CommandData(ctx.Tokens.Command, justifyValue);
        }
    }

    private sealed class TextPosition : ICommand
    {
        public void BeginExecute(in CommandExecutionContext ctx)
        {
            
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
        public void BeginExecute(in CommandExecutionContext ctx)
        {
            
        }

        public CommandData? Parse(in CommandParseContext ctx)
        {
            var sz = ctx.Tokens.AsInt(0);
            return new CommandData(ctx.Tokens.Command, sz);
        }
    }

    private sealed class PlaySoundEffect : ICommand
    {
        public void BeginExecute(in CommandExecutionContext ctx)
        {
            
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
        public void BeginExecute(in CommandExecutionContext ctx)
        {
            
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
        
        public void BeginExecute(in CommandExecutionContext ctx)
        {
            var p = new Params(ctx.Vm.CurrentTime + ctx.Reader.ReadFloat(), ctx.Reader.ReadBoolean());
            ctx.ContinueWith(p, Frame);
        }

        private static void Frame(CommandContinuationContext<Params> ctx)
        {
            if (ctx.Vm.FastForward)
            {
                ctx.Complete();
                return;
            }
            
            if (ctx.Params.FinishTime >= ctx.Vm.CurrentTime)
                ctx.Complete();
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
        public void BeginExecute(in CommandExecutionContext ctx)
        {
            
        }

        public CommandData? Parse(in CommandParseContext ctx)
        {
            return new CommandData(ctx.Tokens.Command);
        }
    }

    private sealed class DebugPrint : ICommand
    {
        public void BeginExecute(in CommandExecutionContext ctx)
        {
            Log.Info(ctx.Reader.ReadString());
            ctx.Complete();
        }

        public CommandData? Parse(in CommandParseContext ctx)
        {
            return new CommandData(ctx.Tokens.Command, ctx.Tokens.AsString(0));
        }
    }

    private sealed class JumpToLabel : ICommand
    {
        public void BeginExecute(in CommandExecutionContext ctx)
        {
            
        }

        public CommandData? Parse(in CommandParseContext ctx)
        {
            var label = ctx.Tokens.AsString(0);
            return new CommandData(ctx.Tokens.Command, label);
        }
    }
    
    private sealed class EndScenario : ICommand
    {
        public void BeginExecute(in CommandExecutionContext ctx)
        {
            ctx.Vm.EndScenario();
        }

        public CommandData? Parse(in CommandParseContext ctx)
        {
            return new CommandData(ctx.Tokens.Command);
        }
    }
}
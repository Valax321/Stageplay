namespace Radish.Scenario;

public static class RuntimeBuiltinCommands
{
    public static IReadOnlyDictionary<string, ICommand> Table = new Dictionary<string, ICommand>
    {
        {"debugprint", new DebugPrint()},
        {"msg", new ShowMessage()},
        {"end", new EndScenario()},
        {"wait", new WaitForSeconds()},
        {"waitfor", new WaitForSeconds()}
    };

    private sealed class ShowMessage : ICommand
    {
        public void BeginExecute(in CommandExecutionContext ctx)
        {
            
        }

        public CommandData? Parse(in CommandParseContext ctx)
        {
            return new CommandData(ctx.Tokens.Command, ctx.Tokens.AsString(0));
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
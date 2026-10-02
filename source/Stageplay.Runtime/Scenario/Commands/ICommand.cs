namespace Radish.Scenario.Commands;

public interface ICommand
{
    CommandReturn BeginExecute(in CommandExecutionContext ctx);
    
    CommandData? Parse(in CommandParseContext ctx) 
        => DefaultCommandParseImpl.Parse(in ctx);
}

internal static class DefaultCommandParseImpl
{
    public static CommandData? Parse(in CommandParseContext ctx)
    {
        var args = new List<ArgumentValue>();
        foreach (var (i, arg) in ctx.ExpectedArguments.Index())
        {
            args.Add(arg switch
            {
                CommandArgumentType.Byte => ArgumentValue.OfByte((byte)ctx.Tokens.AsInt(i)),
                CommandArgumentType.Integer => ArgumentValue.OfInteger(ctx.Tokens.AsInt(i)),
                CommandArgumentType.Float => ArgumentValue.OfFloat(ctx.Tokens.AsFloat(i)),
                CommandArgumentType.String => ArgumentValue.OfString(MakeStringIndexArg(i, in ctx)),
                CommandArgumentType.Boolean => ArgumentValue.OfBoolean(ctx.Tokens.AsInt(i) > 0),
                _ => throw new ArgumentOutOfRangeException(null, "Unknown CommandArgumentType")
            });
        }

        return new CommandData(ctx.Tokens.Command, args);
    }

    private static int MakeStringIndexArg(int i, in CommandParseContext ctx)
    {
        var str = ctx.Tokens.AsString(i);
        return ctx.GetStringIndex(str);
    }
}
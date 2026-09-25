namespace Radish.Scenario.Commands;

public interface ICommand
{
    CommandReturn BeginExecute(in CommandExecutionContext ctx);
    CommandData? Parse(in CommandParseContext ctx);
}

namespace Radish.Scenario.Commands;

public class CommandParseException(int errorCode, string message) : Exception(message)
{
    public int ErrorCode => errorCode;
}

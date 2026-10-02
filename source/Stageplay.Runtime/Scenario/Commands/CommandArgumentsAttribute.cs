using JetBrains.Annotations;

namespace Radish.Scenario.Commands;

[PublicAPI]
[AttributeUsage(AttributeTargets.Class)]
public sealed class CommandArgumentsAttribute : Attribute
{
    public IReadOnlyList<CommandArgumentType> Arguments { get; }
    
    public CommandArgumentsAttribute(string arguments)
    {
        var l = new List<CommandArgumentType>();
        foreach (var c in arguments)
        {
            l.Add(c switch
            {
                'i' => CommandArgumentType.Integer,
                'b' => CommandArgumentType.Boolean,
                's' => CommandArgumentType.String,
                'f' => CommandArgumentType.Float,
                'u' => CommandArgumentType.Byte,
                _ => throw new ArgumentException($"Unsupported argument type \"{c}\"", nameof(arguments))
            });
        }

        Arguments = l;
    }
}
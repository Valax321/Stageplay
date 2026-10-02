using Radish.Scenario;
using Radish.Scenario.Commands;

namespace Radish.ContentBuilder.Scenario;

/// <summary>
/// Base class for bytecode arguments.
/// </summary>
internal static class BytecodeCommandValue
{
    extension(in ArgumentValue val)
    {
        public byte[] ToBytes()
        {
            return val.Type switch
            {
                CommandArgumentType.Byte => [val.AsByte],
                CommandArgumentType.Integer => BitConverter.GetBytes(val.AsInteger),
                CommandArgumentType.Float => BitConverter.GetBytes(val.AsFloat),
                CommandArgumentType.String => BitConverter.GetBytes(val.AsStringIndex),
                CommandArgumentType.Boolean => BitConverter.GetBytes(val.AsBoolean ? 1 : 0),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}
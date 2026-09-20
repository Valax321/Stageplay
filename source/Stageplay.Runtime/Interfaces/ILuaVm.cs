using Lua;

namespace Radish;

/// <summary>
/// Interface to a Lua virtual machine.
/// </summary>
public interface ILuaVm
{
    /// <summary>
    /// The top level state for this VM.
    /// </summary>
    LuaState State { get; }
}
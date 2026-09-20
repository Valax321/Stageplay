using JetBrains.Annotations;
using Lua;

namespace Radish;

/// <summary>
/// Interface to a Lua virtual machine.
/// </summary>
[PublicAPI]
public interface ILuaVm
{
    /// <summary>
    /// The top level state for this VM.
    /// </summary>
    LuaState State { get; }

    /// <summary>
    /// Loads a module synchronously and executes the loaded script.
    /// </summary>
    /// <param name="moduleName">The name of the module to load.</param>
    /// <returns>True if the module executed successfully, otherwise false.</returns>
    public bool LoadAndExecuteModule(string moduleName);

    /// <summary>
    /// Searches the global table for the named function and returns it, if found.
    /// </summary>
    /// <param name="name">The name of the function to find.</param>
    /// <returns>The lua function, or null if it was not present.</returns>
    public LuaFunction? FindGlobalFunction(string name);
    
    /// <summary>
    /// Calls the given lua function synchronously.
    /// </summary>
    /// <param name="func">The function to call.</param>
    /// <param name="args">List of arguments to pass to the function.</param>
    /// <returns>The lua function's return values. Usually this will only be one element.</returns>
    public LuaValue[]? CallSync(LuaFunction func, params ReadOnlySpan<LuaValue> args);
}
using Lua;

namespace Radish.Lua;

/// <summary>
/// Provides a Lua module to a VM.
/// </summary>
public interface ILuaModule
{
    /// <summary>
    /// Creates a new module from this provider.
    /// </summary>
    /// <param name="name">The name of the module created.</param>
    /// <returns>The module data.</returns>
    public LuaModule CreateModule(string name);
}
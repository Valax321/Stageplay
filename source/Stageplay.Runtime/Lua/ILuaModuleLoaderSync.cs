using Lua;

namespace Radish.Lua;

/// <summary>
/// A <see cref="ILuaModuleLoader"/> that can also load modules synchronously.
/// </summary>
public interface ILuaModuleLoaderSync : ILuaModuleLoader
{
    /// <summary>
    /// Loads a lua module synchronously.
    /// </summary>
    /// <param name="moduleName">The name of the module to load.</param>
    /// <returns>The loaded module.</returns>
    /// <seealso cref="ILuaModuleLoader.LoadAsync"/>
    LuaModule Load(string moduleName);
}
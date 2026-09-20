using JetBrains.Annotations;
using Radish.Lua;

namespace Radish;

/// <summary>
/// Provides concrete typed resource providers for the runtime.
/// </summary>
[PublicAPI]
public interface IStageplayResources
{
    /// <summary>
    /// Asset provider for lua modules.
    /// </summary>
    public IResourceProvider<ILuaModule> LuaModules { get; }
}
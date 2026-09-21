using Microsoft.Extensions.DependencyInjection;
using Radish.Lua;
using Radish.Lua.Impl;
using LuaBytecodeModule = Radish.Resources.LuaBytecodeModule;

namespace Radish.Content;

internal sealed class ResourceProviderImpl(StageplayRuntime app) : IResourcesProvider
{
    public IResourceProvider<ILuaModule> LuaModules { get; } =
        new FosterResourceProvider<ILuaModule, LuaBytecodeModule>(app.Services.GetRequiredService<ContentManager>());
}
using Lua;
using Microsoft.Extensions.DependencyInjection;
using Radish.Content;

namespace Radish.Lua.Impl;

internal sealed class FosterLuaModuleLoader(StageplayRuntime app) : ILuaModuleLoaderSync
{
    private readonly Lazy<ContentManager> _content = app.Services.GetRequiredService<Lazy<ContentManager>>();

    public bool Exists(string moduleName)
    {
        var path = $"scripts/{moduleName.Replace('/', '.')}";
        return _content.Value.Exists<Resources.LuaBytecodeModule>(path);
    }

    public async ValueTask<LuaModule> LoadAsync(string moduleName, CancellationToken cancellationToken = new())
    {
        var path = $"scripts/{moduleName.Replace('/', '.')}";
        var bc = await _content.Value.LoadAsync<Resources.LuaBytecodeModule>(path, cancellationToken);
        return bc.CreateModule(moduleName);
    }

    public LuaModule Load(string moduleName)
    {
        var path = $"scripts/{moduleName.Replace('/', '.')}";
        var bc = _content.Value.Load<Resources.LuaBytecodeModule>(path);
        return bc.CreateModule(moduleName);
    }
}
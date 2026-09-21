using Lua;
using Radish.Content;

namespace Radish.Lua.Impl;

internal sealed class FosterLuaModuleLoader(StageplayRuntime app) : ILuaModuleLoaderSync
{
    private readonly ContentManager _content = app.Content;

    public bool Exists(string moduleName)
    {
        var path = $"scripts/{moduleName.Replace('/', '.')}";
        return _content.Exists<Resources.LuaBytecodeModule>(path);
    }

    public async ValueTask<LuaModule> LoadAsync(string moduleName, CancellationToken cancellationToken = new())
    {
        var path = $"scripts/{moduleName.Replace('/', '.')}";
        var bc = await _content.LoadAsync<Resources.LuaBytecodeModule>(path, cancellationToken);
        return bc.CreateModule(moduleName);
    }

    public LuaModule Load(string moduleName)
    {
        var path = $"scripts/{moduleName.Replace('/', '.')}";
        var bc = _content.Load<Resources.LuaBytecodeModule>(path);
        return bc.CreateModule(moduleName);
    }
}
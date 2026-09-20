using Lua;
using Radish.Lua;

namespace Radish.Foster.Lua;

internal sealed class FosterLuaModuleLoader(StageplayApp app) : ILuaModuleLoaderSync
{
    public bool Exists(string moduleName)
    {
        var path = $"scripts/{moduleName.Replace('/', '.')}";
        return app.Content.Exists<LuaBytecodeModule>(path);
    }

    public async ValueTask<LuaModule> LoadAsync(string moduleName, CancellationToken cancellationToken = new())
    {
        var path = $"scripts/{moduleName.Replace('/', '.')}";
        var bc = await app.Content.LoadAsync<LuaBytecodeModule>(path, cancellationToken);
        return bc.CreateModule(moduleName);
    }

    public LuaModule Load(string moduleName)
    {
        var path = $"scripts/{moduleName.Replace('/', '.')}";
        var bc = app.Content.Load<LuaBytecodeModule>(path);
        return bc.CreateModule(moduleName);
    }
}
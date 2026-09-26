using Lua;
using Radish.Content;
using Radish.Resources;

namespace Radish.Lua.Impl;

internal sealed class FosterLuaModuleLoader(StageplayRuntime app) : ILuaModuleLoaderSync
{
    private readonly ContentManager _content = app.Content;

    public bool Exists(string moduleName)
    {
        var path = $"scripts/{moduleName.Replace('/', '.')}.lua";
        return _content.FileExists(path);
    }

    public async ValueTask<LuaModule> LoadAsync(string moduleName, CancellationToken cancellationToken = new())
    {
        var path = $"scripts/{moduleName.Replace('/', '.')}.lua";
        await using var fs = _content.OpenReadOrThrow(path);
        var bc = await LuaBytecodeModule.LoadAsync(fs);
        return bc.CreateModule(moduleName);
    }

    public LuaModule Load(string moduleName)
    {
        var path = $"scripts/{moduleName.Replace('/', '.')}";
        using var fs = _content.OpenReadOrThrow(path);
        var bc = LuaBytecodeModule.Load(fs);
        return bc.CreateModule(moduleName);
    }
}
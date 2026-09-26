using Lua;
using Radish.Content;
using Radish.Resources;

namespace Radish.Lua.Impl;

internal sealed class FosterLuaModuleLoader(StageplayRuntime app) : ILuaModuleLoaderSync
{
    private readonly ContentManager _content = app.Content;

    private static string GetModulePath(string moduleName) 
        => $"scripts/{moduleName.Replace('/', '.')}.lua";

    public bool Exists(string moduleName)
    {
        return _content.FileExists(GetModulePath(moduleName));
    }

    public async ValueTask<LuaModule> LoadAsync(string moduleName, CancellationToken cancellationToken = new())
    {
        await using var fs = _content.OpenReadOrThrow(GetModulePath(moduleName));
        var bc = await LuaBytecodeModule.LoadAsync(fs);
        return bc.CreateModule(moduleName);
    }

    public LuaModule Load(string moduleName)
    {
        using var fs = _content.OpenReadOrThrow(GetModulePath(moduleName));
        var bc = LuaBytecodeModule.Load(fs);
        return bc.CreateModule(moduleName);
    }
}
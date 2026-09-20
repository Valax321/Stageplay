using Lua;
using Microsoft.Xna.Framework.Content;
using Radish.Lua;

namespace Radish.MonoGame.Lua;

internal sealed class GameLuaModuleLoader(StageplayGame game) : ILuaModuleLoaderSync
{
    private readonly Dictionary<string, LuaBytecode> _loadedModules = [];
    
    public bool Exists(string moduleName)
    {
        if (!_loadedModules.TryGetValue(moduleName, out var module))
        {
            var fixedUpName = $"scripts/{moduleName.Replace('.', '/')}";
            try
            {
                module = game.Content.Load<LuaBytecode>(fixedUpName);
                _loadedModules.Add(moduleName, module);
            }
            catch (ContentLoadException)
            {
                return false;
            }
        }

        return true;
    }

    public ValueTask<LuaModule> LoadAsync(string moduleName, CancellationToken cancellationToken = new()) 
        => new(Load(moduleName));

    public LuaModule Load(string moduleName)
    {
        if (!_loadedModules.TryGetValue(moduleName, out var module))
            throw new Exception("LoadAsync called for module that wasn't loaded. This is a bug.");

        return module.CreateModule(moduleName);
    }
}
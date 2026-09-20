using Lua;
using Lua.Platforms;
using Microsoft.Extensions.DependencyInjection;

namespace Radish.Lua;

internal sealed class LuaVm : ILuaVm, IDisposable
{
    public LuaState State { get; }

    public LuaVm(IServiceProvider services)
    {
        State = LuaState.Create(services.GetRequiredService<LuaPlatform>());
        State.ModuleLoader = services.GetRequiredService<ILuaModuleLoader>();
    }

    public void Dispose()
    {
        State.Dispose();
    }
}
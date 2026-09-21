using JetBrains.Annotations;
using Lua;
using Lua.Platforms;
using Radish.Lua.Impl;

namespace Radish.Lua;

[PublicAPI]
public sealed class LuaVM : IDisposable
{
    public LuaState State { get; }

    internal LuaVM(StageplayRuntime app)
    {
        State = LuaState.Create(new LuaPlatform(
            new FosterLuaFilesystem(app),
            new FosterOsEnvironment(app),
            new FosterLuaStandardIO(),
            TimeProvider.System
        ));
        State.ModuleLoader = new FosterLuaModuleLoader(app);

        SetupGlobals();
    }

    private void SetupGlobals()
    {
        var env = State.Environment;
        env["print"] = LuaStaticVmFunctions.LogInfo;
        env["warn"] = LuaStaticVmFunctions.LogWarning;
        env["error"] = LuaStaticVmFunctions.LogError;
    }

    internal void LoadMainModule()
    {
        if (LoadAndExecuteModule("main"))
        {
            var mainFunc = FindGlobalFunction("main");
            if (mainFunc is not null)
                CallSync(mainFunc);
        }
    }

    public bool LoadAndExecuteModule(string moduleName)
    {
        if (!State.ModuleLoader!.Exists(moduleName))
            return false;

        var module = ((ILuaModuleLoaderSync)State.ModuleLoader!).Load(moduleName);
        var c = module.Type switch
        {
            LuaModuleType.Text => State.Load(module.ReadText(), module.Name),
            LuaModuleType.Bytes => State.Load(module.ReadBytes(), module.Name),
            _ => throw new ArgumentOutOfRangeException(null, "Invalid module type")
        };

        try
        {
            var t = State.ExecuteAsync(c);
            if (t.IsCompleted)
                return true;

            t.AsTask().Wait();
            return true;
        }
        catch (LuaRuntimeException ex)
        {
            Log.Error($"Failed to load module \"{moduleName}\": {ex.Message}");
            return false;
        }
    }

    public LuaFunction? FindGlobalFunction(string name)
    {
        if (State.Environment.TryGetValue(name, out var val) && val.TryRead(out LuaFunction func))
        {
            return func;
        }

        return null;
    }

    public LuaValue[]? CallSync(LuaFunction func, params ReadOnlySpan<LuaValue> args)
    {
        try
        {
            var t = State.CallAsync(func, args);
            if (t.IsCompleted)
                return t.Result;

            var tt = t.AsTask();

            tt.Wait();
            return tt.Result;
        }
        catch (LuaRuntimeException ex)
        {
            Log.Error($"Lua CallSync error: {ex.Message}");
            return null;
        }
    }

    public void Dispose()
    {
        State.Dispose();
    }
}
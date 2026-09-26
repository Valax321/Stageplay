using JetBrains.Annotations;
using Lua;
using Lua.Platforms;
using Lua.Standard;
using Radish.Lua.Impl;

namespace Radish.Lua;

/// <summary>
/// Manages a global Lua state for the runtime.
/// Lua is used to handle various game-specific tasks without requiring custom C# code.
/// </summary>
[PublicAPI]
public sealed class LuaVM : IDisposable
{
    /// <summary>
    /// The lua state associated with this VM.
    /// </summary>
    public LuaState State { get; }
    
    private readonly StageplayRuntime _app;
    
    internal LuaVM(StageplayRuntime app)
    {
        _app = app;
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
        State.OpenBasicLibrary();
        State.OpenModuleLibrary();
        State.OpenDebugLibrary();
        State.OpenMathLibrary();
        State.OpenBitwiseLibrary();
        State.OpenStringLibrary();
        State.OpenTableLibrary();
        
        var env = State.Environment;
        env["print"] = LuaStaticVmFunctions.LogInfo;
        env["warn"] = LuaStaticVmFunctions.LogWarning;
        env["error"] = LuaStaticVmFunctions.LogError;
        
        env["audio"] = new LuaAudioBridge(_app);
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

    /// <summary>
    /// Loads a module from disk and executes it.
    /// Note that 'executing' a module really just means the stuff at the global scope (function definitions, global variables etc.).
    /// To actually call a Lua function directly, use <see cref="CallSync"/>.
    /// </summary>
    /// <param name="moduleName">The name of the module. The actual script file is loaded from <c>scripts/[moduleName].luac</c></param>
    /// <returns><see langword="true"/> if the module was loaded successfully, otherwise <see langword="false"/>.</returns>
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

    /// <summary>
    /// Searches the lua global state for a function with the given name and returns it.
    /// </summary>
    /// <param name="name">The name of the function to find.</param>
    /// <returns>The function if found, or <see langword="null"/> if a global with the given name did not exist, or if it was something other than a function.</returns>
    public LuaFunction? FindGlobalFunction(string name)
    {
        if (State.Environment.TryGetValue(name, out var val) && val.TryRead(out LuaFunction func))
        {
            return func;
        }

        return null;
    }

    /// <summary>
    /// Synchronously calls a Lua function and returns its return values.
    /// </summary>
    /// <param name="func">The function to call.</param>
    /// <param name="args">Parameters that should be passed to the Lua function.</param>
    /// <returns>The function's returned values. If the runtime encountered a runtime error executing the function, <see langword="null"/> is returned.</returns>
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

    /// <inheritdoc/>
    public void Dispose()
    {
        State.Dispose();
    }
}
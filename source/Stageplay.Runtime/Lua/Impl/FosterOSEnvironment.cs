using Lua.Platforms;

namespace Radish.Lua.Impl;

internal sealed class FosterOsEnvironment(StageplayRuntime app) : ILuaOsEnvironment
{
    public string? GetEnvironmentVariable(string name)
    {
        return Environment.GetEnvironmentVariable(name);
    }

    public ValueTask Exit(int exitCode, CancellationToken cancellationToken)
    {
        app.Exit();
        return new ValueTask();
    }

    public double GetTotalProcessorTime()
    {
        return app.Time.Seconds;
    }
}
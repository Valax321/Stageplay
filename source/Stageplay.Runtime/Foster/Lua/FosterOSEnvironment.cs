using Lua.Platforms;

namespace Radish.Foster.Lua;

internal sealed class FosterOsEnvironment(StageplayApp app) : ILuaOsEnvironment
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
using Lua.Platforms;

namespace Radish.MonoGame.Lua;

internal sealed class GameLuaOSEnvironment(StageplayGame game) : ILuaOsEnvironment
{
    public string? GetEnvironmentVariable(string name)
    {
        return Environment.GetEnvironmentVariable(name);
    }

    public ValueTask Exit(int exitCode, CancellationToken cancellationToken)
    {
        game.Exit();
        return new ValueTask();
    }

    public double GetTotalProcessorTime()
    {
        return game.TimeProvider.TotalElapsed.TotalSeconds;
    }
}
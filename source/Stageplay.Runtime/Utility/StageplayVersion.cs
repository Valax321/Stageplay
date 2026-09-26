using System.Reflection;

namespace Radish.Utility;

public static class StageplayVersion
{
    public static string Version { get; }
    
    static StageplayVersion()
    {
        var a = Assembly.GetExecutingAssembly();
        var attr = a.GetCustomAttribute<AssemblyFileVersionAttribute>();
        Version = attr?.Version ?? string.Empty;
    }
}
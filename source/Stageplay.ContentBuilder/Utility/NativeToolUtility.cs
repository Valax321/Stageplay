using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Radish.ContentBuilder.Utility;

public static class NativeToolUtility
{
    public static async Task<int> RunNativeTool(string name, string[] args, bool verbose = false)
    {
        var toolPath = Path.Combine(AppContext.BaseDirectory, "tools", DetermineToolDirectory(), RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? $"{name}.exe" : name);
        var proc = Process.Start(new ProcessStartInfo(toolPath, args)
        {
            RedirectStandardOutput = !verbose,
            RedirectStandardError = !verbose,
        });
        
        if (proc is null)
            throw new Exception("Process.Start returned null");

        await proc.WaitForExitAsync();
        return proc.ExitCode;
    }

    private static string DetermineToolDirectory()
    {
        var sb = new StringBuilder();
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            sb.Append("win");
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            sb.Append("osx");
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            sb.Append("linux");
        else
            throw new PlatformNotSupportedException();

        sb.Append('-');
        sb.Append(RuntimeInformation.OSArchitecture.ToString().ToLowerInvariant());

        return sb.ToString();
    }
}
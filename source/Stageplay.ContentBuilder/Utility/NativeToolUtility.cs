using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using JetBrains.Annotations;

namespace Radish.ContentBuilder.Utility;

/// <summary>
/// Utility methods for running a process asynchronously and getting the resulting exit code.
/// </summary>
[PublicAPI]
public static class NativeToolUtility
{
    /// <summary>
    /// Runs the given program located in tools/{rid} within the application's <see cref="AppContext.BaseDirectory"/>.
    /// </summary>
    /// <param name="name">The name of the program to execute.</param>
    /// <param name="args">List of command line arguments to pass to the program.</param>
    /// <param name="verbose">If true, the invoked process's output will be sent redirected to this application's stdout/stderr.</param>
    /// <returns>The exit code returned by the process.</returns>
    /// <exception cref="InvalidOperationException">Thrown if <see cref="Process.Start(ProcessStartInfo)"/> internally. returns null.</exception>
    public static async Task<int> RunNativeTool(string name, string[] args, bool verbose = false)
    {
        var toolPath = Path.Combine(AppContext.BaseDirectory, "tools", DetermineToolDirectory(), RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? $"{name}.exe" : name);
        var proc = Process.Start(new ProcessStartInfo(toolPath, args)
        {
            RedirectStandardOutput = !verbose,
            RedirectStandardError = !verbose,
        });
        
        if (proc is null)
            throw new InvalidOperationException("Process.Start returned null");

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
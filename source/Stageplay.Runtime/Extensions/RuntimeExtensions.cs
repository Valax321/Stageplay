using JetBrains.Annotations;

namespace Radish;

[PublicAPI]
public static class RuntimeExtensions
{
    extension(StageplayRuntime runtime)
    {
        /// <summary>
        /// Entrypoint that should be used for self-hosted runtime backends (e.g. Monogame) that own their own main loop.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the system host does not implement <see cref="IStandaloneSystemHost"/>.</exception>
        public void RunWithMainLoop()
        {
            if (runtime.Host is not IStandaloneSystemHost l)
                throw new InvalidOperationException(
                    $"Cannot use {nameof(RunWithMainLoop)} without a ${nameof(IStandaloneSystemHost)}");
        
            l.Run();
        }
    }
}
using System.Drawing;
using JetBrains.Annotations;

namespace Radish;

/// <summary>
/// Provides some basic info about the game being run on Stageplay.
/// </summary>
/// <param name="Organization">The display name of the developer of the game.</param>
/// <param name="ApplicationName">The display name of the game itself.</param>
/// <param name="DesignSize">
/// The screen resolution game assets were designed to run at. At different resolutions, the UI will be scaled up/down to this size.
/// Also controls the maximum aspect ratio the game will run at.
/// </param>
[PublicAPI]
public record GameInfo(string Organization, string ApplicationName, Size DesignSize);

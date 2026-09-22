using Foster.Framework;
using JetBrains.Annotations;

namespace Radish;

/// <summary>
/// Provides some basic info about the game being run on Stageplay.
/// </summary>
/// <param name="Organization">The display name of the developer of the game.</param>
/// <param name="ApplicationName">The display name of the game itself.</param>
/// <param name="ApplicationIdentifier">The reverse domain name identifier for this game.</param>
/// <param name="Version">The version of this application.</param>
/// <param name="DesignSize">
/// The screen resolution game assets were designed to run at. At different resolutions, the UI will be scaled up/down to this size.
/// Also controls the maximum aspect ratio the game will run at.
/// </param>
/// <param name="ContentDirectory">The content directory used for loading assets. Must be a relative path.</param>
[PublicAPI]
public record GameInfo(string Organization, string ApplicationName, string ApplicationIdentifier, Version Version, Point2 DesignSize, string ContentDirectory = "Content");

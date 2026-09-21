using System.Drawing;
using Radish;
using Radish.Steamworks;

var gameInfo = new GameInfo("Audrey Castillo",
    "Stageplay Foster Example",
    "zone.audrey.foster-example",
    Version.Parse(GitVersionInformation.AssemblySemVer),
    new Size(1920, 1080)
);

var builder = StageplayRuntimeBuilder.Create(args)
    .WithGameInfo(gameInfo)
    .WithSteamworks(480)
    .WithCoreSystemComponents<ExampleGame>();

using var runtime = builder.Build();
runtime.Run();

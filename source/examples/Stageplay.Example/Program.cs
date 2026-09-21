using System.Drawing;
using Radish;
using Radish.Steamworks;

var gameInfo = new GameInfo("Audrey Castillo",
    "Stageplay Foster Example",
    "zone.audrey.foster-example",
    Version.Parse(GitVersionInformation.AssemblySemVer),
    new Size(1920, 1080)
);

using var runtime = StageplayRuntime.CreateWithGame<ExampleGame>(args, gameInfo)
    .WithSteamworks(480)
    .Build();
    
runtime.Run();

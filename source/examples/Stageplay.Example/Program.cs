using System.Reflection;
using Foster.Framework;
using Radish;
using Radish.Steamworks;

var gameInfo = new GameInfo("Audrey Castillo",
    "Stageplay Foster Example",
    "zone.audrey.foster-example",
    Assembly.GetExecutingAssembly().GetName().Version!,
    new Point2(640, 480) // Test assets from The Closet are this size
);

using var runtime = StageplayRuntime.CreateWithGame<ExampleGame>(args, gameInfo)
    .WithSteamworks(480, errorIfSteamInitFailed: true)
    .Build();
    
runtime.Run();

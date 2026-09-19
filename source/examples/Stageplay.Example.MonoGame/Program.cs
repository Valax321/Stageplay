using Radish;
using Radish.MonoGame;
using Radish.Steamworks;

var gameInfo = new GameInfo("Audrey Castillo", "Stageplay MonoGame Example");

var builder = StageplayRuntimeBuilder.Create(args)
    .WithGameInfo(gameInfo)
    .WithSteamworks(480)
    .WithMonoGameHost();
    
using var runtime = builder.Build();
runtime.RunWithMainLoop();

using Lua;
using Radish.Audio;

namespace Radish.Lua;

[LuaObject]
internal partial class LuaMixerGroupHandle(AudioMixerGroup group)
{
    [LuaMember("name")]
    private string Name => group.Name;

    [LuaMember("volume")]
    private float Volume
    {
        get => group.Volume;
        set => group.Volume = value;
    }
}

[LuaObject]
internal partial class LuaAudioClipHandle(AudioClip clip)
{
    public WeakReference<AudioClip> Clip { get; } = new(clip);
}

[LuaObject]
internal partial class LuaAudioBridge
{
    private readonly StageplayRuntime _app;

    internal LuaAudioBridge(StageplayRuntime app)
    {
        _app = app;
    }

    [LuaMember("addMixerGroup")]
    private LuaMixerGroupHandle AddMixerGroup(string name)
    {
        var mixGroup = _app.SoundManager.AddMixerGroup(name);
        return new LuaMixerGroupHandle(mixGroup);
    }

    [LuaMember("precacheSound")]
    private LuaAudioClipHandle? PrecacheAudioClip(string name, bool decompressInMemory)
    {
        var path = $"sounds/{name}.ogg";
        var snd = _app.SoundManager.PrecacheAudioClip(path, decompressInMemory);
        if (snd is null)
            return null;
        
        return new LuaAudioClipHandle(snd);
    }
}
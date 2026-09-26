using JetBrains.Annotations;
using Radish.Audio;
using Radish.Content;

namespace Radish.Scenario;

[PublicAPI]
public sealed class ScenarioSoundManager
{
    private readonly AudioDevice _device;
    private readonly ContentManager _content;

    private readonly Dictionary<string, AudioMixerGroup> _mixerGroups = [];
    private readonly Dictionary<string, AudioClip> _precachedClips = [];
    
    private readonly AudioInstance?[] _channels = new AudioInstance?[8];

    internal ScenarioSoundManager(AudioDevice device, ContentManager content)
    {
        _device = device;
        _content = content;
    }

    public AudioMixerGroup AddMixerGroup(string name)
    {
        if (_mixerGroups.TryGetValue(name, out var group))
            return group;

        group = new AudioMixerGroup(_device, name);
        _mixerGroups.Add(name, group);
        return group;
    }

    public AudioClip? PrecacheAudioClip(string path, bool decompressInMemory)
    {
        if (_precachedClips.TryGetValue(path, out var clip))
            return clip;

        using var s = _content.OpenRead(path);
        if (s is null)
        {
            Log.Warning($"Could not precache {path}, it does not exist");
            return null;
        }

        clip = new AudioClip(_device, s, new AudioClip.DecodeSettings(decompressInMemory, false));
        _precachedClips.Add(path, clip);
        return clip;
    }
}
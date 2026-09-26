using JetBrains.Annotations;
using static SDL3.SDL_mixer;

namespace Radish.Audio;

/// <summary>
/// Controls the volume of a group of sounds.
/// </summary>
[PublicAPI]
public sealed class AudioMixerGroup
{
    /// <summary>
    /// The name of the mixer group.
    /// </summary>
    public string Name { get; }
    
    internal string TagName { get; }

    /// <summary>
    /// The volume of the mixer group.
    /// </summary>
    public float Volume
    {
        get;
        set
        {
            field = Math.Clamp(value, 0, 1);
            MIX_SetTagGain(_device.Mixer, TagName, field);
        }
    }

    private readonly AudioDevice _device;
    
    internal AudioMixerGroup(AudioDevice device, string name)
    {
        Name = name;
        TagName = $"_MixerGroup_{Name}";
        _device = device;

        // There is no GetTagGain so we need to manually do this once to keep the value in sync
        Volume = 1;
    }
}
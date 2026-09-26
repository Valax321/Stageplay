namespace Radish.Audio;

internal sealed class AudioStreamInstance : AudioInstance
{
    public AudioStream Stream { get; }

    internal AudioStreamInstance(AudioDevice device, IntPtr track, AudioStream stream, AudioMixerGroup? mixerGroup) :
        base(device, track,
            mixerGroup)
    {
        Stream = stream;
    }
}
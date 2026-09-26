namespace Radish.Audio;

internal sealed class AudioClipInstance : AudioInstance
{
    public AudioClip Source { get; }

    internal AudioClipInstance(AudioDevice device, IntPtr track, AudioClip clip, AudioMixerGroup? mixerGroup) : base(
        device, track, mixerGroup)
    {
        Source = clip;
    }
}
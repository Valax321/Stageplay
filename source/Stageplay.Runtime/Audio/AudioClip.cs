using JetBrains.Annotations;
using Radish.Utility;
using static SDL3.SDL;
using static SDL3.SDL_mixer;

namespace Radish.Audio;

/// <summary>
/// Stores audio samples in memory.
/// </summary>
[PublicAPI]
public sealed class AudioClip : IDisposable
{
    /// <summary>
    /// The length of the audio clip in seconds.
    /// If the length could not be determined, returns 0.
    /// </summary>
    public float Length
    {
        get
        {
            EnsureNotDisposed();
            var frames = MIX_GetAudioDuration(_audio);
            if (frames < 0)
                return 0;

            return MIX_AudioFramesToMS(_audio, frames) / 1000.0f;
        }
    }
    
    internal IntPtr Audio
    {
        get
        {
            EnsureNotDisposed();
            return _audio;
        }
    }
    
    private IntPtr _audio;
    private AudioDevice _device;
    
    /// <summary>
    /// Settings used when decoding the audio data.
    /// </summary>
    /// <param name="DecompressInMemory">If true, the audio data will be stored in its raw format. This uses more memory but reduces the decompression CPU overhead.</param>
    /// <param name="IgnoreLoopPoints">If true, any loop points found in the audio data will be ignored.</param>
    public readonly record struct DecodeSettings(bool DecompressInMemory, bool IgnoreLoopPoints);
    
    /// <summary>
    /// Creates a new AudioClip from the given file.
    /// </summary>
    public AudioClip(AudioDevice device, string path, in DecodeSettings settings) : this(device, File.OpenRead(path), in settings)
    {
    }

    /// <summary>
    /// Creates a new AudioClip from the given stream.
    /// </summary>
    public AudioClip(AudioDevice device, Stream stream, in DecodeSettings settings)
    {
        _device = device;
        var props = SDL_CreateProperties();
        try
        {
            SDL_SetBooleanProperty(props, MIX_PROP_AUDIO_LOAD_PREDECODE_BOOLEAN, settings.DecompressInMemory);
            SDL_SetBooleanProperty(props, MIX_PROP_AUDIO_LOAD_IGNORE_LOOPS_BOOLEAN, settings.IgnoreLoopPoints);
            SDL_SetPointerProperty(props, MIX_PROP_AUDIO_LOAD_PREFERRED_MIXER_POINTER, device.Mixer);
            SDL_SetPointerProperty(props, MIX_PROP_AUDIO_LOAD_IOSTREAM_POINTER,
                SdlStreamIO.CreateIOWrapperForManagedStream(stream));
            SDL_SetBooleanProperty(props, MIX_PROP_AUDIO_LOAD_CLOSEIO_BOOLEAN, true);

            _audio = MIX_LoadAudioWithProperties(props);
            if (_audio == 0)
                throw new AudioException(SDL_GetError());

            device.TrackAudioHandle(_audio);
        }
        finally
        {
            SDL_DestroyProperties(props);
        }
    }
    
    /// <summary>
    /// Creates a new sound instance for playback.
    /// </summary>
    /// <param name="mixerGroup">An optional mixer group that controls the volume of this instance.</param>
    /// <returns>A new audio instance.</returns>
    public AudioInstance CreateInstance(AudioMixerGroup? mixerGroup = null)
    {
        EnsureNotDisposed();
        
        var track = MIX_CreateTrack(_device.Mixer);
        if (track == 0)
            throw new AudioException(SDL_GetError());

        if (!MIX_SetTrackAudio(track, _audio))
            throw new AudioException(SDL_GetError());

        return new AudioClipInstance(_device, track, this, mixerGroup);
    }
    
    /// <inheritdoc/>
    public void Dispose()
    {
        if (_audio != 0)
        {
            _device.UnTrackAudioHandle(_audio);
            MIX_DestroyAudio(_audio);
            _audio = 0;
        }
    }

    private void EnsureNotDisposed()
    {
        if (_audio == 0)
            throw new ObjectDisposedException(nameof(AudioClip), "AudioClip data already disposed");
    }
}
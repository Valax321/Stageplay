using JetBrains.Annotations;
using Radish.Utility;
using static SDL3.SDL;
using static SDL3.SDL_mixer;

namespace Radish.Audio;

/// <summary>
/// AudioStreams play back sound by streaming audio samples from disk in real-time.
/// This makes them ideal for large audio sources such as music playback.
/// AudioStreams can only be used by a single <see cref="AudioInstance"/>, so they are consumed when <see cref="CreateInstance"/> is called.
/// Any further calls to CreateInstance will throw an exception.
/// </summary>
[PublicAPI]
public sealed class AudioStream
{
    /// <summary>
    /// True if the stream is being consumed by an <see cref="AudioInstance"/>.
    /// If so, it cannot be used to create a new instance.
    /// </summary>
    public bool Consumed => _source is null;
    
    private Stream? _source;
    private AudioDevice _device;
    
    /// <summary>
    /// Creates a new audio stream from the given file.
    /// </summary>
    /// <param name="device">The audio device that owns this stream.</param>
    /// <param name="path">The path to the audio file to open.</param>
    public AudioStream(AudioDevice device, string path) : this(device, File.OpenRead(path))
    {
    }

    /// <summary>
    /// Creates a new audio stream from the given <see cref="Stream"/>.
    /// </summary>
    /// <param name="device">The audio device that owns this stream.</param>
    /// <param name="stream">The stream of audio data to read from. The AudioStream will take ownership of this stream and dispose it at the correct time, so it should never be disposed manually.</param>
    public AudioStream(AudioDevice device, Stream stream)
    {
        if (!stream.CanSeek)
            throw new InvalidOperationException("Stream must be seekable to use as an AudioStream");
        
        _device = device;
        _source = stream;
    }
    
    /// <summary>
    /// Creates a new sound instance for playback.
    /// </summary>
    /// <param name="stream">The streaming source to play from.</param>
    /// <param name="mixerGroup">An optional mixer group that controls the volume of this instance.</param>
    /// <returns>A new audio instance.</returns>
    public AudioInstance CreateInstance(AudioStream stream, AudioMixerGroup? mixerGroup = null)
    {
        var io = stream.Consume();
        
        var track = MIX_CreateTrack(_device.Mixer);
        if (track == 0)
            throw new AudioException(SDL_GetError());

        if (!MIX_SetTrackIOStream(track, io, true))
            throw new AudioException(SDL_GetError());

        return new AudioStreamInstance(_device, track, stream, mixerGroup);
    }

    private IntPtr Consume()
    {
        if (_source is null)
            throw new ObjectDisposedException(nameof(AudioStream),
                "AudioStream has already been consumed by an AudioInstance. AudioStreams cannot be reused once consumed.");
        
        var s = SdlStreamIO.CreateIOWrapperForManagedStream(_source);
        _source = null;
        return s;
    }
}
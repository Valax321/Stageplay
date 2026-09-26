using System.Runtime.InteropServices.Marshalling;
using JetBrains.Annotations;
using static SDL3.SDL_mixer;
using static SDL3.SDL;

namespace Radish.Audio;

/// <summary>
/// Represents a single instance of a sound. AudioInstances are created through <see cref="AudioDevice.CreateInstance(Radish.Audio.AudioClip,Radish.Audio.AudioMixerGroup?)"/>.
/// An audio instance must be explicitly set to play by calling <see cref="Play"/> before any sound will be heard.
/// </summary>
[PublicAPI]
public abstract class AudioInstance : IDisposable
{
    internal IntPtr Track
    {
        get
        {
            EnsureNotDisposed();
            return _track;
        }
    }
    
    private IntPtr _track;
    
    /// <summary>
    /// The <see cref="AudioMixerGroup"/> associated with this sound.
    /// </summary>
    public AudioMixerGroup? MixerGroup { get; }
    
    /// <summary>
    /// Tags assigned to this sound.
    /// </summary>
    public IReadOnlyCollection<string> Tags => _tags;
    private HashSet<string> _tags = [];

    /// <summary>
    /// The volume of this sound.
    /// Mixer group volumes apply in addition to this value.
    /// </summary>
    public float Volume
    {
        get
        {
            EnsureNotDisposed();
            return MIX_GetTrackGain(_track);
        }
        set
        {
            EnsureNotDisposed();
            MIX_SetTrackGain(_track, Math.Clamp(value, 0, 1));
        }
    }

    /// <summary>
    /// Controls whether this sound instance is currently paused.
    /// </summary>
    /// <remarks><see cref="IsPlaying"/> is distinct from this. A track can be unpaused and also not playing (if it is stopped or before play is called).</remarks>
    public bool IsPaused
    {
        get
        {
            EnsureNotDisposed();
            return MIX_TrackPaused(_track);
        }
        set
        {
            EnsureNotDisposed();
            if (value)
                MIX_PauseTrack(_track);
            else
                MIX_ResumeTrack(_track);
        }
    }

    /// <summary>
    /// True if the track has been started via <see cref="Play"/>.
    /// </summary>
    public bool IsPlaying
    {
        get
        {
            EnsureNotDisposed();
            return MIX_TrackPlaying(_track);
        }
    }

    /// <summary>
    /// True if the track has been disposed.
    /// </summary>
    public bool Disposed => _track == 0;

    /// <summary>
    /// Controls the stereo panning.
    /// -1 is fully to the left, 0 is centre, 1 is fully to the right.
    /// </summary>
    public float StereoPan
    {
        get;
        set
        {
            EnsureNotDisposed();
            field = Math.Clamp(value, -1, 1);
            
            // Remap -1,1 to 0,1 for each side
            var gains = new MIX_StereoGains
            {
                left = field * -0.5f + 1,
                right = field * 0.5f + 1
            };
            
            // Because the above calulcation would result in the centre pan being (0.5, 0.5)
            // we add back up to 0.5 gain depending on how close to the centre the channel is.
            var distFromZero = 1 - Math.Abs(field);
            gains.left += distFromZero * 0.5f;
            gains.right += distFromZero * 0.5f;

            // Ensure the gain never increases above 1
            gains.left = Math.Clamp(gains.left, 0, 1);
            gains.right = Math.Clamp(gains.right, 0, 1);

            MIX_SetTrackStereo(_track, in gains);
        }
    }

    private AudioDevice _device;
    
    internal AudioInstance(AudioDevice device, IntPtr track, AudioMixerGroup? mixerGroup)
    {
        _device = device;
        _track = track;
        MixerGroup = mixerGroup;

        device.TrackTrackHandle(_track);

        if (mixerGroup is not null)
            AddTag(mixerGroup.TagName);
    }

    /// <summary>
    /// Plays the sound instance.
    /// </summary>
    /// <param name="loop">If true, the sound will loop endlessly.</param>
    /// <param name="fadeInTime">The time in seconds over which the sound will fade in.</param>
    public void Play(bool loop = false, float fadeInTime = 0)
    {
        EnsureNotDisposed();

        var props = SDL_CreateProperties();
        SDL_SetNumberProperty(props, MIX_PROP_PLAY_FADE_IN_MILLISECONDS_NUMBER, (long)(fadeInTime * 1000));
        SDL_SetNumberProperty(props, MIX_PROP_PLAY_LOOPS_NUMBER, loop ? -1 : 0);
        if (!MIX_PlayTrack(_track, props))
        {
            Log.Error($"Failed to play audio track: {SDL_GetError()}");    
        }
        
        SDL_DestroyProperties(props);
    }

    /// <summary>
    /// Stops playing a sound instance with an optional fadeout.
    /// </summary>
    /// <param name="fadeOutTime">The time in seconds to fade out over.</param>
    /// <param name="disposeOnStop">If true, the sound instance's <see cref="Dispose"/> method will be called once the sound has fully faded out.</param>
    public void Stop(float fadeOutTime = 0, bool disposeOnStop = false)
    {
        EnsureNotDisposed();

        if (disposeOnStop)
            MIX_SetTrackStoppedCallback(_track, (_, _) => Dispose(), 0);
        else
            MIX_SetTrackStoppedCallback(_track, null, 0);
        
        if (!MIX_StopTrack(_track, (long)(fadeOutTime * 1000)))
        {
            Log.Error($"Failed to stop audio track: {SDL_GetError()}");
        }
    }
    
    /// <summary>
    /// Adds a string tag to the sound, which can be used to pause/resume/stop multiple sounds in a group at once.
    /// </summary>
    /// <param name="tag">The tag to add.</param>
    public void AddTag(string tag)
    {
        EnsureNotDisposed();
        
        if (MIX_TagTrack(_track, tag))
            UpdateTagsList();
    }

    /// <summary>
    /// Removes a string tag from the sound.
    /// </summary>
    /// <param name="tag">The tag to remove.</param>
    public void RemoveTag(string tag)
    {
        EnsureNotDisposed();

        MIX_UntagTrack(_track, tag);
        UpdateTagsList();
    }

    private unsafe void UpdateTagsList()
    {
        EnsureNotDisposed();
        var tagsPtr = (byte**)MIX_GetTrackTags(_track, out var count);
        if (tagsPtr == null)
            throw new AudioException(SDL_GetError());
            
        _tags.Clear();
        for (var i = 0; i < count; ++i)
            _tags.Add(Utf8StringMarshaller.ConvertToManaged(tagsPtr[i]) ?? string.Empty);

        SDL_free((IntPtr)tagsPtr);
    }

    private void EnsureNotDisposed()
    {
        if (_track == 0)
            throw new ObjectDisposedException(nameof(AudioInstance), "Track has already been disposed");
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        if (_track != 0)
        {
            _device.UnTrackTrackHandle(_track);
            MIX_DestroyTrack(_track);
            _track = 0;
        }
    }
}
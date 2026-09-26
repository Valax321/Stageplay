using JetBrains.Annotations;
using static SDL3.SDL;
using static SDL3.SDL_mixer;

namespace Radish.Audio;

/// <summary>
/// Manages runtime audio playback.
/// </summary>
[PublicAPI]
public sealed class AudioDevice : IDisposable
{
    /// <summary>
    /// The master audio playback volume.
    /// </summary>
    public float Volume
    {
        get
        {
            EnsureNotDisposed();
            return MIX_GetMixerGain(_mixer);
        }
        set
        {
            EnsureNotDisposed();
            MIX_SetMixerGain(_mixer, Math.Clamp(value, 0, 1));
        }
    }
    
    internal IntPtr Mixer
    {
        get
        {
            EnsureNotDisposed();
            return _mixer;
        }
    }
    
    private IntPtr _mixer;

    private HashSet<IntPtr> AudioHandles { get; } = [];
    private HashSet<IntPtr> TrackHandles { get; } = [];

    internal void TrackAudioHandle(IntPtr audio)
    {
        lock (AudioHandles)
        {
            AudioHandles.Add(audio);
        }
    }
    
    internal void UnTrackAudioHandle(IntPtr audio)
    {
        lock (AudioHandles)
        {
            AudioHandles.Remove(audio);
        }
    }
    
    internal void TrackTrackHandle(IntPtr track)
    {
        lock (TrackHandles)
        {
            TrackHandles.Add(track);
        }
    }
    
    internal void UnTrackTrackHandle(IntPtr track)
    {
        lock (TrackHandles)
        {
            TrackHandles.Remove(track);
        }
    }
    
    internal AudioDevice()
    {
        if (!MIX_Init())
            throw new Exception($"Failed to init SDL_mixer: {SDL_GetError()}");

        _mixer = MIX_CreateMixerDevice(SDL_AUDIO_DEVICE_DEFAULT_PLAYBACK, 0);
        if (_mixer == 0)
            throw new Exception($"Failed to create mixer device: {SDL_GetError()}");
    }

    /// <summary>
    /// Stops all playing sound instances with the given tag.
    /// </summary>
    /// <remarks>This will invalidate ALL <see cref="AudioInstance"/>s currently allocated with the tag.
    /// Any further functions calls on them will throw exceptions!</remarks>
    /// <param name="tag">The tag to stop. If null, then all sounds will be stopped regardless of tag.</param>
    /// <param name="fadeOutTime">Time in seconds to fade out tracks with.</param>
    public void StopAll(string? tag = null, float fadeOutTime = 0)
    {
        EnsureNotDisposed();
        var t = (int)(fadeOutTime * 1000);
        if (tag is not null)
        {
            MIX_StopTag(_mixer, tag, t);
        }
        else
        {
            MIX_StopAllTracks(_mixer, t);
        }
    }

    /// <summary>
    /// Pauses all sound instances with the given tag.
    /// </summary>
    /// <param name="tag">The tag to pause. If null, then all sounds will be paused regardless of tag.</param>
    public void PauseAll(string? tag = null)
    {
        EnsureNotDisposed();
        if (tag != null)
        {
            MIX_PauseTag(_mixer, tag);
        }
        else
        {
            MIX_PauseAllTracks(_mixer);
        }
    }

    /// <summary>
    /// Resumes all paused sound instances with the given tag.
    /// </summary>
    /// <param name="tag">The tag to resume. If null, then all sounds will be resumed regardless of tag.</param>
    public void ResumeAll(string? tag = null)
    {
        EnsureNotDisposed();
        if (tag == null)
        {
            MIX_ResumeAllTracks(_mixer);
        }
        else
        {
            MIX_ResumeTag(_mixer, tag);
        }
    }
    
    /// <inheritdoc/>
    public void Dispose()
    {
        if (_mixer != 0)
        {
            lock (TrackHandles)
            {
                foreach (var t in TrackHandles)
                    MIX_DestroyTrack(t);
                TrackHandles.Clear();
            }

            lock (AudioHandles)
            {
                foreach (var a in AudioHandles)
                    MIX_DestroyAudio(a);
                AudioHandles.Clear();
            }
            
            MIX_DestroyMixer(_mixer);
            _mixer = 0;
        }
        
        MIX_Quit();
    }

    private void EnsureNotDisposed()
    {
        if (_mixer == 0)
            throw new ObjectDisposedException(nameof(AudioDevice), "AudioDevice mixer has already been destroyed");
    }
}
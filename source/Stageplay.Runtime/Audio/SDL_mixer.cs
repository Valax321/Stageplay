using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using JetBrains.Annotations;

// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
namespace SDL3;

using static SDL;

/*
 * Unlike the base SDL bindings, these are written by hand.
 * This isn't the best approach, but SDL_mixer is a lot smaller than core SDL and SDL3-CS doesn't have any
 * existing bindings. We can't also safely rely on a completely different set of SDL bindings that *do* include mixer
 * since they will drag in a second set of core bindings and will probably cause issues.
 *
 * SDL_Mixer seems to be pretty set in its API and not updated a lot, so writing by hand will suffice.
 *
 * If we ever do a custom backend that isn't Foster-based, we can use a more comprehensive set of C# bindings
 * and this file can be removed.
 */

/// <summary>
/// https://wiki.libsdl.org/SDL3_mixer/CategorySDLMixer
/// </summary>
[PublicAPI]
internal static partial class SDL_mixer
{
    private const string NativeLibraryName = "SDL_mixer";
    
    #region Constants

    public const uint SDL_AUDIO_DEVICE_DEFAULT_PLAYBACK = 0xFFFFFFFFu;

    public const long MIX_DURATION_UNKNOWN = -1;
    public const long MIX_DURATION_INFINITE = -2;
    
    #endregion
    
    #region Properties
    
    public const string MIX_PROP_METADATA_TITLE_STRING = "SDL_mixer.metadata.title";
    public const string MIX_PROP_METADATA_ARTIST_STRING = "SDL_mixer.metadata.artist";
    public const string MIX_PROP_METADATA_ALBUM_STRING = "SDL_mixer.metadata.album";
    public const string MIX_PROP_METADATA_COPYRIGHT_STRING = "SDL_mixer.metadata.copyright";
    public const string MIX_PROP_METADATA_TRACK_NUMBER = "SDL_mixer.metadata.track";
    public const string MIX_PROP_METADATA_TOTAL_TRACKS_NUMBER = "SDL_mixer.metadata.total_tracks";
    public const string MIX_PROP_METADATA_YEAR_NUMBER = "SDL_mixer.metadata.year";
    public const string MIX_PROP_METADATA_DURATION_FRAMES_NUMBER = "SDL_mixer.metadata.duration_frames";
    public const string MIX_PROP_METADATA_DURATION_INFINITE_BOOLEAN = "SDL_mixer.metadata.duration_infinite";
    
    public const string MIX_PROP_AUDIO_LOAD_IOSTREAM_POINTER = "SDL_mixer.audio.load.iostream";
    public const string MIX_PROP_AUDIO_LOAD_CLOSEIO_BOOLEAN = "SDL_mixer.audio.load.closeio";
    public const string MIX_PROP_AUDIO_LOAD_PREDECODE_BOOLEAN = "SDL_mixer.audio.load.predecode";
    public const string MIX_PROP_AUDIO_LOAD_PREFERRED_MIXER_POINTER = "SDL_mixer.audio.load.preferred_mixer";
    public const string MIX_PROP_AUDIO_LOAD_SKIP_METADATA_TAGS_BOOLEAN = "SDL_mixer.audio.load.skip_metadata_tags";
    public const string MIX_PROP_AUDIO_LOAD_IGNORE_LOOPS_BOOLEAN = "SDL_mixer.audio.load.ignore_loops";
    public const string MIX_PROP_AUDIO_DECODER_STRING = "SDL_mixer.audio.decoder";

    public const string MIX_PROP_MIXER_DEVICE_NUMBER = "SDL_mixer.mixer.device";
    
    public const string MIX_PROP_PLAY_LOOPS_NUMBER = "SDL_mixer.play.loops";
    public const string MIX_PROP_PLAY_MAX_FRAME_NUMBER = "SDL_mixer.play.max_frame";
    public const string MIX_PROP_PLAY_MAX_MILLISECONDS_NUMBER = "SDL_mixer.play.max_milliseconds";
    public const string MIX_PROP_PLAY_START_FRAME_NUMBER = "SDL_mixer.play.start_frame";
    public const string MIX_PROP_PLAY_START_MILLISECOND_NUMBER = "SDL_mixer.play.start_millisecond";
    public const string MIX_PROP_PLAY_START_ORDER_NUMBER = "SDL_mixer.play.start_order";
    public const string MIX_PROP_PLAY_LOOP_START_FRAME_NUMBER = "SDL_mixer.play.loop_start_frame";
    public const string MIX_PROP_PLAY_LOOP_START_MILLISECOND_NUMBER = "SDL_mixer.play.loop_start_millisecond";
    public const string MIX_PROP_PLAY_FADE_IN_FRAMES_NUMBER = "SDL_mixer.play.fade_in_frames";
    public const string MIX_PROP_PLAY_FADE_IN_MILLISECONDS_NUMBER = "SDL_mixer.play.fade_in_milliseconds";
    public const string MIX_PROP_PLAY_FADE_IN_START_GAIN_FLOAT = "SDL_mixer.play.fade_in_start_gain";
    public const string MIX_PROP_PLAY_APPEND_SILENCE_FRAMES_NUMBER = "SDL_mixer.play.append_silence_frames";
    public const string MIX_PROP_PLAY_APPEND_SILENCE_MILLISECONDS_NUMBER = "SDL_mixer.play.append_silence_milliseconds";
    public const string MIX_PROP_PLAY_HALT_WHEN_EXHAUSTED_BOOLEAN = "SDL_mixer.play.halt_when_exhausted";
    
    #endregion
    
    #region Functions

    /// <summary>
    /// https://wiki.libsdl.org/SDL3_mixer/MIX_AudioFramesToMS
    /// </summary>
    [LibraryImport(NativeLibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial long MIX_AudioFramesToMS(IntPtr audio, long frames);

    /// <summary>
    /// https://wiki.libsdl.org/SDL3_mixer/MIX_AudioMSToFrames
    /// </summary>
    [LibraryImport(NativeLibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial long MIX_AudioMSToFrames(IntPtr audio, long ms);

    /// <summary>
    /// https://wiki.libsdl.org/SDL3_mixer/MIX_CreateAudioDecoder
    /// </summary>
    [LibraryImport(NativeLibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial IntPtr MIX_CreateAudioDecoder(string path, uint props);

    /// <summary>
    /// https://wiki.libsdl.org/SDL3_mixer/MIX_CreateAudioDecoder_IO
    /// </summary>
    [LibraryImport(NativeLibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial IntPtr MIX_CreateAudioDecoder_IO(IntPtr io, SDLBool closeio, uint props);

    /// <summary>
    /// https://wiki.libsdl.org/SDL3_mixer/MIX_CreateGroup
    /// </summary>
    [LibraryImport(NativeLibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial IntPtr MIX_CreateGroup(IntPtr mixer);

    /// <summary>
    /// https://wiki.libsdl.org/SDL3_mixer/MIX_CreateMixer
    /// </summary>
    [LibraryImport(NativeLibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial IntPtr MIX_CreateMixer(IntPtr spec);

    /// <summary>
    /// https://wiki.libsdl.org/SDL3_mixer/MIX_CreateMixerDevice
    /// </summary>
    [LibraryImport(NativeLibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial IntPtr MIX_CreateMixerDevice(uint devid, IntPtr spec);

    /// <summary>
    /// https://wiki.libsdl.org/SDL3_mixer/MIX_CreateSineWaveAudio
    /// </summary>
    [LibraryImport(NativeLibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial IntPtr MIX_CreateSineWaveAudio(IntPtr mixer, int hz, float amplitude, long ms);

    /// <summary>
    /// https://wiki.libsdl.org/SDL3_mixer/MIX_CreateTrack
    /// </summary>
    [LibraryImport(NativeLibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial IntPtr MIX_CreateTrack(IntPtr mixer);

    /// <summary>
    /// https://wiki.libsdl.org/SDL3_mixer/MIX_DecodeAudio
    /// </summary>
    [LibraryImport(NativeLibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int MIX_DecodeAudio(IntPtr audiodecoder, IntPtr buffer, int buflen, IntPtr spec);

    /// <summary>
    /// https://wiki.libsdl.org/SDL3_mixer/MIX_DestroyAudio
    /// </summary>
    [LibraryImport(NativeLibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void MIX_DestroyAudio(IntPtr audio);

    /// <summary>
    /// https://wiki.libsdl.org/SDL3_mixer/MIX_DestroyAudioDecoder
    /// </summary>
    [LibraryImport(NativeLibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void MIX_DestroyAudioDecoder(IntPtr audiodecoder);

    /// <summary>
    /// https://wiki.libsdl.org/SDL3_mixer/MIX_DestroyGroup
    /// </summary>
    [LibraryImport(NativeLibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void MIX_DestroyGroup(IntPtr group);

    /// <summary>
    /// https://wiki.libsdl.org/SDL3_mixer/MIX_DestroyMixer
    /// </summary>
    [LibraryImport(NativeLibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void MIX_DestroyMixer(IntPtr mixer);

    /// <summary>
    /// https://wiki.libsdl.org/SDL3_mixer/MIX_DestroyTrack
    /// </summary>
    [LibraryImport(NativeLibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void MIX_DestroyTrack(IntPtr track);

    /// <summary>
    /// https://wiki.libsdl.org/SDL3_mixer/MIX_FramesToMS
    /// </summary>
    [LibraryImport(NativeLibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial long MIX_FramesToMS(int sample_rate, long frames);

    /// <summary>
    /// https://wiki.libsdl.org/SDL3_mixer/MIX_Generate
    /// </summary>
    [LibraryImport(NativeLibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int MIX_Generate(IntPtr mixer, IntPtr buffer, int buflen);
    
    /// <summary>
    /// https://wiki.libsdl.org/SDL3_mixer/MIX_GetAudioDecoder
    /// </summary>
    [LibraryImport(NativeLibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(SDLOwnedStringMarshaller))]
    public static partial string MIX_GetAudioDecoder(int index);

    /// <summary>
    /// https://wiki.libsdl.org/SDL3_mixer/MIX_GetAudioDecoderFormat
    /// </summary>
    [LibraryImport(NativeLibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial SDLBool MIX_GetAudioDecoderFormat(IntPtr audiodecoder, out IntPtr spec);

    /// <summary>
    /// https://wiki.libsdl.org/SDL3_mixer/MIX_GetAudioDecoderProperties
    /// </summary>
    [LibraryImport(NativeLibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial uint MIX_GetAudioDecoderProperties(IntPtr audiodecoder);

    /// <summary>
    /// https://wiki.libsdl.org/SDL3_mixer/MIX_GetAudioDuration
    /// </summary>
    [LibraryImport(NativeLibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial long MIX_GetAudioDuration(IntPtr audio);

    [LibraryImport(NativeLibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial SDLBool MIX_GetAudioFormat(IntPtr audio, out IntPtr spec);

    [LibraryImport(NativeLibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial uint MIX_GetAudioProperties(IntPtr audio);

    [LibraryImport(NativeLibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial IntPtr MIX_GetGroupMixer(IntPtr group);

    [LibraryImport(NativeLibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial uint MIX_GetGroupProperties(IntPtr group);

    [LibraryImport(NativeLibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial SDLBool MIX_GetMixerFormat(IntPtr mixer, out IntPtr spec);

    [LibraryImport(NativeLibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial float MIX_GetMixerFrequencyRatio(IntPtr mixer);

    [LibraryImport(NativeLibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial float MIX_GetMixerGain(IntPtr mixer);

    [LibraryImport(NativeLibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial uint MIX_GetMixerProperties(IntPtr mixer);

    [LibraryImport(NativeLibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int MIX_GetNumAudioDecoders();

    [LibraryImport(NativeLibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial IntPtr MIX_GetTaggedTracks(IntPtr mixer, string tag, out int count);

    [LibraryImport(NativeLibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial SDLBool MIX_GetTrack3DPosition(IntPtr track, out MIX_Point3D position);

    [LibraryImport(NativeLibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial IntPtr MIX_GetTrackAudio(IntPtr track);

    [LibraryImport(NativeLibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial IntPtr MIX_GetTrackAudioStream(IntPtr track);

    [LibraryImport(NativeLibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial long MIX_GetTrackFadeFrames(IntPtr track);

    [LibraryImport(NativeLibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial float MIX_GetTrackFrequencyRatio(IntPtr track);

    [LibraryImport(NativeLibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial float MIX_GetTrackGain(IntPtr track);

    [LibraryImport(NativeLibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int MIX_GetTrackLoops(IntPtr track);

    [LibraryImport(NativeLibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial IntPtr MIX_GetTrackMixer(IntPtr track);

    [LibraryImport(NativeLibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial long MIX_GetTrackPlaybackPosition(IntPtr track);

    [LibraryImport(NativeLibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial uint MIX_GetTrackProperties(IntPtr track);

    [LibraryImport(NativeLibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial long MIX_GetTrackRemaining(IntPtr track);

    [LibraryImport(NativeLibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial IntPtr MIX_GetTrackTags(IntPtr track, out int count);

    [LibraryImport(NativeLibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial SDLBool MIX_Init();

    [LibraryImport(NativeLibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial IntPtr MIX_LoadAudio(IntPtr mixer, string path, SDLBool predecode);

    [LibraryImport(NativeLibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial IntPtr MIX_LoadAudio_IO(IntPtr mixer, IntPtr io, SDLBool predecode, SDLBool closeio);

    [LibraryImport(NativeLibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial IntPtr MIX_LoadAudioNoCopy(IntPtr mixer, IntPtr data, nuint datalen, SDLBool free_when_done);

    [LibraryImport(NativeLibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial IntPtr MIX_LoadRawAudio(IntPtr mixer, IntPtr data, nuint datalen, IntPtr spec);

    [LibraryImport(NativeLibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial IntPtr MIX_LoadRawAudio_IO(IntPtr mixer, IntPtr io, IntPtr spec, SDLBool closeio);

    [LibraryImport(NativeLibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial IntPtr MIX_LoadRawAudioNoCopy(IntPtr mixer, IntPtr data, nuint datalen, IntPtr spec, SDLBool free_when_done);

    [LibraryImport(NativeLibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void MIX_LockMixer(IntPtr mixer);

    [LibraryImport(NativeLibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial long MIX_MSToFrames(int sample_rate, long ms);

    [LibraryImport(NativeLibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial SDLBool MIX_PauseAllTracks(IntPtr mixer);

    [LibraryImport(NativeLibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial SDLBool MIX_PauseTag(IntPtr mixer, string tag);

    [LibraryImport(NativeLibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial SDLBool MIX_PauseTrack(IntPtr track);

    [LibraryImport(NativeLibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial SDLBool MIX_PlayAudio(IntPtr mixer, IntPtr audio);

    [LibraryImport(NativeLibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial SDLBool MIX_PlayTag(IntPtr mixer, string tag, uint options);

    [LibraryImport(NativeLibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial SDLBool MIX_PlayTrack(IntPtr track, uint options);

    [LibraryImport(NativeLibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void MIX_Quit();

    [LibraryImport(NativeLibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial SDLBool MIX_ResumeAllTracks(IntPtr mixer);

    [LibraryImport(NativeLibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial SDLBool MIX_ResumeTag(IntPtr mixer, string tag);

    [LibraryImport(NativeLibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial SDLBool MIX_ResumeTrack(IntPtr track);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void MIX_GroupMixCallback(IntPtr userdata, IntPtr group, IntPtr spec, IntPtr pcm, int samples);

    [LibraryImport(NativeLibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial SDLBool MIX_SetGroupPostMixCallback(IntPtr group, MIX_GroupMixCallback cb, IntPtr userdata);

    [LibraryImport(NativeLibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial SDLBool MIX_SetMixerFrequencyRatio(IntPtr mixer, float ratio);

    [LibraryImport(NativeLibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial SDLBool MIX_SetMixerGain(IntPtr mixer, float gain);
    
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void MIX_PostMixCallback(IntPtr userdata, IntPtr mixer, IntPtr spec, IntPtr pcm, int samples);

    [LibraryImport(NativeLibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial SDLBool MIX_SetPostMixCallback(IntPtr mixer, MIX_PostMixCallback cb, IntPtr userdata);

    [LibraryImport(NativeLibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial SDLBool MIX_SetTagGain(IntPtr mixer, string tag, float gain);

    [LibraryImport(NativeLibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial SDLBool MIX_SetTrack3DPosition(IntPtr track, in MIX_Point3D position);

    [LibraryImport(NativeLibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial SDLBool MIX_SetTrackAudio(IntPtr track, IntPtr audio);

    [LibraryImport(NativeLibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial SDLBool MIX_SetTrackAudioStream(IntPtr track, IntPtr stream);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void MIX_TrackMixCallback(IntPtr userdata, IntPtr track, IntPtr spec, IntPtr pcm, int samples);

    [LibraryImport(NativeLibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial SDLBool MIX_SetTrackCookedCallback(IntPtr track, MIX_TrackMixCallback cb, IntPtr userdata);

    [LibraryImport(NativeLibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial SDLBool MIX_SetTrackFrequencyRatio(IntPtr track, float ratio);

    [LibraryImport(NativeLibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial SDLBool MIX_SetTrackGain(IntPtr track, float gain);

    [LibraryImport(NativeLibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial SDLBool MIX_SetTrackGroup(IntPtr track, IntPtr group);

    [LibraryImport(NativeLibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial SDLBool MIX_SetTrackIOStream(IntPtr track, IntPtr io, SDLBool closeio);

    [LibraryImport(NativeLibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial SDLBool MIX_SetTrackLoops(IntPtr track, int num_loops);

    [LibraryImport(NativeLibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial SDLBool MIX_SetTrackOutputChannelMap(IntPtr track, ReadOnlySpan<int> chmap, int count);

    [LibraryImport(NativeLibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial SDLBool MIX_SetTrackPlaybackPosition(IntPtr track, long frames);

    [LibraryImport(NativeLibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial SDLBool MIX_SetTrackRawCallback(IntPtr track, MIX_TrackMixCallback cb, IntPtr userdata);

    [LibraryImport(NativeLibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial SDLBool MIX_SetTrackRawIOStream(IntPtr track, IntPtr io, IntPtr spec, SDLBool closeio);

    [LibraryImport(NativeLibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial SDLBool MIX_SetTrackStereo(IntPtr track, in MIX_StereoGains gains);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void MIX_TrackStoppedCallback(IntPtr userdata, IntPtr track);
    
    [LibraryImport(NativeLibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial SDLBool MIX_SetTrackStoppedCallback(IntPtr stack, MIX_TrackStoppedCallback cb, IntPtr userdata);

    [LibraryImport(NativeLibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial SDLBool MIX_StopAllTracks(IntPtr mixer, long fade_out_ms);

    [LibraryImport(NativeLibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial SDLBool MIX_StopTag(IntPtr mixer, string tag, long fade_out_ms);

    [LibraryImport(NativeLibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial SDLBool MIX_StopTrack(IntPtr track, long fade_out_ms);

    [LibraryImport(NativeLibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial SDLBool MIX_TagTrack(IntPtr track, string tag);

    [LibraryImport(NativeLibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial long MIX_TrackFramesToMS(IntPtr track, long frames);

    [LibraryImport(NativeLibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial long MIX_TrackMSToFrames(IntPtr track, long ms);

    [LibraryImport(NativeLibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial SDLBool MIX_TrackPaused(IntPtr track);

    [LibraryImport(NativeLibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial SDLBool MIX_TrackPlaying(IntPtr track);

    [LibraryImport(NativeLibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void MIX_UnlockMixer(IntPtr mixer);

    [LibraryImport(NativeLibraryName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void MIX_UntagTrack(IntPtr track, string? tag);
    
    /// <summary>
    /// https://wiki.libsdl.org/SDL3_mixer/MIX_Version
    /// </summary>
    [LibraryImport(NativeLibraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int MIX_Version();
    
    #endregion
    
    #region Structs
    
    /// <summary>
    /// https://wiki.libsdl.org/SDL3_mixer/MIX_Point3D
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct MIX_Point3D
    {
        public float x;
        public float y;
        public float z;
    }

    /// <summary>
    /// https://wiki.libsdl.org/SDL3_mixer/MIX_StereoGains
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct MIX_StereoGains
    {
        public float left;
        public float right;
    }
    
    #endregion
}
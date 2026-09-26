using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static SDL3.SDL;
// ReSharper disable InconsistentNaming

namespace Radish.Utility;

internal static class SdlStreamIO
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate long IO_SizeCallback(IntPtr userdata);
    
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate long IO_SeekCallback(IntPtr userdata, long offset, SDL_IOWhence whence);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private unsafe delegate nuint IO_ReadCallback(IntPtr userdata, void* ptr, nuint size, SDL_IOStatus* status);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private unsafe delegate nuint IO_WriteCallback(IntPtr userdata, void* ptr, nuint size, SDL_IOStatus* status);
    
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private unsafe delegate SDLBool IO_FlushCallback(IntPtr userdata, SDL_IOStatus* status);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate SDLBool IO_CloseCallback(IntPtr userdata);

    private struct InterfaceFuncs
    {
        public IO_SizeCallback size;
        public IO_SeekCallback seek;
        public IO_ReadCallback read;
        public IO_WriteCallback write;
        public IO_FlushCallback flush;
        public IO_CloseCallback close;
    }

    public static IntPtr CreateIOWrapperForManagedStream(Stream stream)
    {
        var impl = new SDL_IOStreamInterface
        {
            version = (uint)Unsafe.SizeOf<SDL_IOStreamInterface>(),
            size = Marshal.GetFunctionPointerForDelegate(Callbacks.size),
            seek = Marshal.GetFunctionPointerForDelegate(Callbacks.seek),
            read = Marshal.GetFunctionPointerForDelegate(Callbacks.read),
            write = Marshal.GetFunctionPointerForDelegate(Callbacks.write),
            flush = Marshal.GetFunctionPointerForDelegate(Callbacks.flush),
            close = Marshal.GetFunctionPointerForDelegate(Callbacks.close)
        };

        var streamHandle = GCHandle.Alloc(stream);
        return SDL_OpenIO(ref impl, GCHandle.ToIntPtr(streamHandle));
    }
    
    private static readonly unsafe InterfaceFuncs Callbacks = new()
    {
        size = Size_Impl,
        seek = Seek_Impl,
        read = Read_Impl,
        write = Write_Impl,
        flush = Flush_Impl,
        close = Close_Impl
    };

    private static long Size_Impl(IntPtr userdata)
    {
        var handle = GCHandle.FromIntPtr(userdata);
        if (handle.Target is not Stream stream)
        {
            SDL_SetError("Could not get .NET Stream from userdata GCHandle");
            return -1;
        }

        return stream.Length;
    }
    
    private static long Seek_Impl(IntPtr userdata, long offset, SDL_IOWhence whence)
    {
        var handle = GCHandle.FromIntPtr(userdata);
        if (handle.Target is not Stream stream)
        {
            SDL_SetError("Could not get .NET Stream from userdata GCHandle");
            return -1;
        }

        if (!stream.CanSeek)
        {
            SDL_SetError("Cannot seek stream");
            return -1;
        }

        return stream.Seek(offset, whence switch
        {
            SDL_IOWhence.SDL_IO_SEEK_SET => SeekOrigin.Begin,
            SDL_IOWhence.SDL_IO_SEEK_CUR => SeekOrigin.Current,
            SDL_IOWhence.SDL_IO_SEEK_END => SeekOrigin.End,
            _ => throw new ArgumentOutOfRangeException(nameof(whence), whence, null)
        });
    }
    
    private static unsafe nuint Read_Impl(IntPtr userdata, void* ptr, UIntPtr size, SDL_IOStatus* status)
    {
        var handle = GCHandle.FromIntPtr(userdata);
        if (handle.Target is not Stream stream)
        {
            *status = SDL_IOStatus.SDL_IO_STATUS_ERROR;
            SDL_SetError("Could not get .NET Stream from userdata GCHandle");
            return 0;
        }

        if (!stream.CanRead)
        {
            *status = SDL_IOStatus.SDL_IO_STATUS_WRITEONLY;
            return 0;
        }

        var buffer = new Span<byte>(ptr, (int)size);
        var readBytes = stream.Read(buffer);
        if (readBytes < (int)size)
            *status = SDL_IOStatus.SDL_IO_STATUS_EOF;

        return (nuint)readBytes;
    }
    
    private static unsafe UIntPtr Write_Impl(IntPtr userdata, void* ptr, UIntPtr size, SDL_IOStatus* status)
    {
        var handle = GCHandle.FromIntPtr(userdata);
        if (handle.Target is not Stream stream)
        {
            *status = SDL_IOStatus.SDL_IO_STATUS_ERROR;
            SDL_SetError("Could not get .NET Stream from userdata GCHandle");
            return 0;
        }

        if (!stream.CanWrite)
        {
            *status = SDL_IOStatus.SDL_IO_STATUS_READONLY;
            return 0;
        }

        var startPos = stream.Position;
        var buffer = new ReadOnlySpan<byte>(ptr, (int)size);
        stream.Write(buffer);
        
        var writtenCount = stream.Position - startPos;
        if (writtenCount < (long)size)
            *status = SDL_IOStatus.SDL_IO_STATUS_EOF;

        return (nuint)writtenCount;
    }
    
    private static unsafe SDLBool Flush_Impl(IntPtr userdata, SDL_IOStatus* status)
    {
        var handle = GCHandle.FromIntPtr(userdata);
        if (handle.Target is not Stream stream)
        {
            *status = SDL_IOStatus.SDL_IO_STATUS_ERROR;
            SDL_SetError("Could not get .NET Stream from userdata GCHandle");
            return false;
        }
        
        stream.Flush();
        return true;
    }
    
    private static SDLBool Close_Impl(IntPtr userdata)
    {
        var handle = GCHandle.FromIntPtr(userdata);
        if (handle.Target is not Stream stream)
        {
            SDL_SetError("Could not get .NET Stream from userdata GCHandle");
            return false;
        }
        
        handle.Free();
        stream.Dispose();
        return true;
    }
}
using System.Runtime.InteropServices;
using Xui.Runtime.Android.Composition;

namespace Xui.Runtime.Android.Interop;

/// <summary>Small C ABI. No Android or Java managed bindings are used by the renderer.</summary>
internal static unsafe partial class AndroidBridge
{
    private const string Library = "xui_android";
    [LibraryImport(Library, EntryPoint = "xui_create")]
    internal static partial nint Create(nint javaVm, nint activity, nint user, delegate* unmanaged[Cdecl]<nint, int, long, byte*, int, int> callback);
    [LibraryImport(Library, EntryPoint = "xui_buffer")]
    private static partial byte* Buffer(nint host, int capacity);
    [LibraryImport(Library, EntryPoint = "xui_call")]
    private static partial int Call(nint host, int operation, int length);
    [LibraryImport(Library, EntryPoint = "xui_destroy")]
    internal static partial void Destroy(nint host);
    [LibraryImport(Library, EntryPoint = "xui_error")]
    private static partial nint Error(nint host);

    internal static Span<byte> Invoke(nint host, int operation, WireBuffer input, int resultBytes = 0)
    {
        var ptr = Buffer(host, Math.Max(input.Length, resultBytes));
        if (ptr == null)
        {
            var error = Marshal.PtrToStringUTF8(Error(host));
            if (!string.IsNullOrEmpty(error)) throw new InvalidOperationException(error);
            throw new OutOfMemoryException("Android bridge buffer allocation failed.");
        }
        input.Data.CopyTo(new Span<byte>(ptr, input.Length));
        if (Call(host, operation, input.Length) != 0)
            throw new InvalidOperationException(Marshal.PtrToStringUTF8(Error(host)) ?? "Android bridge failure.");
        return new Span<byte>(ptr, resultBytes);
    }
}

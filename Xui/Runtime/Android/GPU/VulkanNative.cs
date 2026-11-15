using System.Runtime.InteropServices;
namespace Xui.Runtime.Android.GPU;

internal static unsafe partial class VulkanNative
{
    private const string Library = "xui_android";
    [LibraryImport(Library, EntryPoint="xui_vk_create")] internal static partial nint Create();
    [LibraryImport(Library, EntryPoint="xui_vk_destroy")] internal static partial void Destroy(nint device);
    [LibraryImport(Library, EntryPoint="xui_vk_error")] private static partial nint Error();
    [LibraryImport(Library, EntryPoint="xui_assets")] internal static partial nint Assets(nint host);
    [LibraryImport(Library, EntryPoint="xui_vk_shader", StringMarshalling=StringMarshalling.Utf8)] internal static partial ulong Shader(nint device,nint assets,string path);
    [LibraryImport(Library, EntryPoint="xui_vk_shader_destroy")] internal static partial void DestroyShader(nint device,ulong shader);
    [LibraryImport(Library, EntryPoint="xui_vk_target_create")] internal static partial nint Target(nint device,nint window);
    [LibraryImport(Library, EntryPoint="xui_vk_target_destroy")] internal static partial void DestroyTarget(nint target);
    [LibraryImport(Library, EntryPoint="xui_vk_begin")] internal static partial int Begin(nint target,int width,int height,float* color);
    [LibraryImport(Library, EntryPoint="xui_vk_pipeline")] internal static partial int Pipeline(nint target,ulong vertex,ulong fragment,int depth,int write,int cull,int stride);
    [LibraryImport(Library, EntryPoint="xui_vk_vertices")] internal static partial int Vertices(nint target,void* data,int length);
    [LibraryImport(Library, EntryPoint="xui_vk_uniforms")] internal static partial int Uniforms(nint target,void* data,int length);
    [LibraryImport(Library, EntryPoint="xui_vk_draw")] internal static partial int Draw(nint target,int count);
    [LibraryImport(Library, EntryPoint="xui_vk_commit")] internal static partial int Commit(nint target);
    internal static Exception Failure() => new InvalidOperationException(Marshal.PtrToStringUTF8(Error()) ?? "Vulkan operation failed.");
    internal static int Check(int result) { if(result<0)throw Failure();return result; }
}

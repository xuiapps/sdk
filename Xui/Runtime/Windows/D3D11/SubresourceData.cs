using System.Runtime.InteropServices;

namespace Xui.Runtime.Windows;

public static partial class D3D11
{
    /// <summary>
    /// Specifies data for initialising a subresource (e.g. initial pixel data for a texture).
    /// Mirrors <c>D3D11_SUBRESOURCE_DATA</c>.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct SubresourceData
    {
        /// <summary>Pointer to the initialisation data.</summary>
        public void* pSysMem;

        /// <summary>Row stride in bytes (width × bytes-per-pixel for a 2D texture).</summary>
        public uint SysMemPitch;

        /// <summary>Slice stride in bytes (unused for 2D textures — set to 0).</summary>
        public uint SysMemSlicePitch;
    }
}

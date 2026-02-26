using System.Runtime.InteropServices;

namespace Xui.Runtime.Windows;

public static partial class D3D11
{
    /// <summary>
    /// Describes a 2D texture resource.
    /// Mirrors <c>D3D11_TEXTURE2D_DESC</c> (44 bytes, fully sequential).
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Texture2DDesc
    {
        public uint Width;
        public uint Height;

        /// <summary>Number of mip levels. 1 = no mipmaps.</summary>
        public uint MipLevels;

        /// <summary>Number of textures in the array. Use 1 for a single texture.</summary>
        public uint ArraySize;

        public DXGI.Format Format;
        public DXGI.SampleDesc SampleDesc;

        /// <summary>
        /// Resource usage. 0 = <c>D3D11_USAGE_DEFAULT</c> (GPU read/write, no CPU access).
        /// </summary>
        public uint Usage;

        public uint BindFlags;
        public uint CPUAccessFlags;
        public uint MiscFlags;
    }
}

using System.Runtime.InteropServices;

namespace Xui.Runtime.Windows;

public static partial class D2D1
{
    /// <summary>
    /// Configures how a <c>ID2D1BitmapBrush</c> tiles and samples the source bitmap.
    /// Mirrors <c>D2D1_BITMAP_BRUSH_PROPERTIES</c> (12 bytes, sequential).
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct BitmapBrushProperties
    {
        /// <summary>
        /// How the bitmap is extended horizontally beyond its bounds.
        /// <c>D2D1_EXTEND_MODE</c>: Clamp=0, Wrap=1, Mirror=2.
        /// </summary>
        public uint ExtendModeX;

        /// <summary>
        /// How the bitmap is extended vertically beyond its bounds.
        /// <c>D2D1_EXTEND_MODE</c>: Clamp=0, Wrap=1, Mirror=2.
        /// </summary>
        public uint ExtendModeY;

        /// <summary>
        /// Sampling quality.
        /// <c>D2D1_BITMAP_INTERPOLATION_MODE</c>: NearestNeighbor=0, Linear=1.
        /// </summary>
        public uint InterpolationMode;
    }
}

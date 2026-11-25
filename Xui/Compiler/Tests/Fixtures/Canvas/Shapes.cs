using System.Runtime.InteropServices;
using CanvasDemo;
using Xui.Core.Canvas;
using Xui.Core.Math2D;

public static class Program
{
    public static int Main()
    {
        NativeCanvas.Clear(0.0625, 0.125, 0.25, 1);
        Rect left = default;
        left.X = 40; left.Y = 100; left.Width = 240; left.Height = 180;
        Color blue = default;
        blue.Red = 0.125f; blue.Green = 0.5f; blue.Blue = 1; blue.Alpha = 1;
        Draw.Fill(ref left, ref blue);
        Rect right = left;
        right.X = left.Right + (NFloat)40;
        Color coral = default;
        coral.Red = 1; coral.Green = 0.375f; coral.Blue = 0.25f; coral.Alpha = 1;
        Draw.Fill(ref right, ref coral);
        Color white = default;
        white.Red = 1; white.Green = 1; white.Blue = 1; white.Alpha = 1;
        Draw.Text("Hello from Xui WASM", 40, 60, 28, ref white);
        return 0;
    }
}

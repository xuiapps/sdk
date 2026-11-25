using System.Runtime.InteropServices;
using CanvasDemo;
using Xui.Core.Canvas;
using Xui.Core.Math2D;

public static class Program
{
    public static int Main()
    {
        NativeCanvas.Clear(0.0625, 0.125, 0.25, 1);
        Color color = default;
        color.Red = 0.25f; color.Green = 0.75f; color.Blue = 1; color.Alpha = 1;
        Rect cell = default; cell.Width = 72; cell.Height = 56;
        for (int row = 0; row < 3; row++)
        {
            for (int column = 0; column < 6; column++)
            {
                cell.X = (NFloat)40 + (NFloat)column * (NFloat)88;
                cell.Y = (NFloat)96 + (NFloat)row * (NFloat)72;
                Draw.Fill(ref cell, ref color);
            }
        }
        Draw.Text("Geometry calculated in C#", 40, 56, 24, ref color);
        return 18;
    }
}

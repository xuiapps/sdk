using System;
using System.Runtime.InteropServices;
using Xui.Core.Canvas;
using Xui.Core.Math2D;

namespace CanvasDemo;

public static class Draw
{
    public static void Fill(ref Rect rectangle, ref Color color) => NativeCanvas.FillRect(
        (double)rectangle.X, (double)rectangle.Y, (double)rectangle.Width, (double)rectangle.Height,
        (double)color.Red, (double)color.Green, (double)color.Blue, (double)color.Alpha);

    public static void Text(string text, NFloat x, NFloat y, NFloat size, ref Color color) => NativeCanvas.FillText(
        text, (double)x, (double)y, (double)size, (double)color.Red, (double)color.Green, (double)color.Blue, (double)color.Alpha);
}

// Explicit host boundary. Tests replace only these exact signatures with drawing imports.
public static class NativeCanvas
{
    public static void Clear(double r, double g, double b, double a) => throw new NotSupportedException();
    public static void FillRect(double x, double y, double width, double height, double r, double g, double b, double a) => throw new NotSupportedException();
    public static void FillText(string text, double x, double y, double size, double r, double g, double b, double a) => throw new NotSupportedException();
}

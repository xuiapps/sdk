using CanvasDemo;
using Xui.Core.Canvas;
using Xui.Core.Math2D;

public interface IPainter { void Paint(ref PaintEvent e); }
public ref struct PaintEvent
{
    public Rect Bounds;
    public Color Color;
    public string Label;
    public IPainter Painter;
}
public class Painter : IPainter
{
    public void Paint(ref PaintEvent e)
    {
        Draw.Fill(ref e.Bounds, ref e.Color);
        e.Bounds.X = 40; e.Bounds.Y = 60;
        Draw.Text(e.Label, e.Bounds.X, e.Bounds.Y, 26, ref e.Color);
    }
}
public static class Program
{
    public static int Main()
    {
        NativeCanvas.Clear(0.0625, 0.125, 0.25, 1);
        PaintEvent e = default;
        e.Bounds.X = 40; e.Bounds.Y = 100; e.Bounds.Width = 520; e.Bounds.Height = 180;
        e.Color.Red = 0.5f; e.Color.Green = 1; e.Color.Blue = 0.75f; e.Color.Alpha = 1;
        e.Label = "Stack event → interface → canvas";
        e.Painter = new Painter();
        e.Painter.Paint(ref e);
        return 42;
    }
}

using System.Runtime.InteropServices;
using CanvasDemo;
using Xui.Core.Canvas;
using Xui.Core.Math2D;

public interface ILayout { Rect Arrange(Rect bounds); }
public class InsetLayout : ILayout
{
    public Rect Arrange(Rect bounds) => bounds.Inset((NFloat)20);
}
public static class Program
{
    static void Paint(Rect bounds, Color color) => Draw.Fill(ref bounds, ref color);

    public static int Main()
    {
        NativeCanvas.Clear(0.0625, 0.125, 0.25, 1);
        Point origin = (0, 0);
        Vector shift = (40, 100);
        origin += shift;
        Rect outer = (origin.X, origin.Y, 520, 180);
        ILayout layout = new InsetLayout();
        Rect inner = layout.Arrange(outer);
        Color blue = new Color((NFloat)0.125, (NFloat)0.5, (NFloat)1, (NFloat)1);
        Color coral = new Color((NFloat)1, (NFloat)0.375, (NFloat)0.25, (NFloat)1);
        Paint(outer, blue);
        Paint(inner, coral);
        Color white = new Color((NFloat)1, (NFloat)1, (NFloat)1, (NFloat)1);
        Draw.Text("Rect values through interface calls", 40, 60, 26, ref white);
        return inner.Width == (NFloat)480 ? 42 : 1;
    }
}

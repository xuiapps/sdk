using System.Runtime.InteropServices;

using Xui.Core.Canvas;
using Xui.Core.DI;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.DevKit.UI.Design;

namespace Xui.Apps.TestApp.Examples.DesignSystem;

/// <summary>
/// Renders color palette swatches for all color groups (Primary, Secondary, Tertiary, Error, Application, Surface).
/// </summary>
internal class ColorGroupSwatchesView : View
{
    private static readonly NFloat Size = 36;
    private static readonly NFloat Gap = 4;
    private static readonly NFloat GroupGap = 16;

    public override int Count => 0;
    public override View this[int index] => throw new IndexOutOfRangeException();

    private static readonly NFloat SwatchBlockH = 18 + Size + Gap + Size;
    private static readonly NFloat RowGap = 16;
    private static readonly NFloat GroupStride = Size * 2 + Gap + GroupGap;

    protected override Size MeasureCore(Size available, IMeasureContext context)
    {
        return new Size(available.Width, SwatchBlockH * 3 + RowGap * 2);
    }

    protected override void RenderCore(IContext context)
    {
        var ds = this.GetService<IDesignSystem>();
        if (ds == null) return;

        NFloat x = this.Frame.X;
        NFloat y = this.Frame.Y;

        // Row 1: Primary, Secondary, Tertiary
        DrawGroup(context, "Primary", ds.Colors.Primary, x, y, Size, Gap);
        DrawGroup(context, "Secondary", ds.Colors.Secondary, x + GroupStride, y, Size, Gap);
        DrawGroup(context, "Tertiary", ds.Colors.Tertiary, x + GroupStride * 2, y, Size, Gap);

        // Row 2: Application, Surface, Neutral
        NFloat row2Y = y + SwatchBlockH + RowGap;
        DrawGroup(context, "Application", ds.Colors.Application, x, row2Y, Size, Gap);
        DrawGroup(context, "Surface", ds.Colors.Surface, x + GroupStride, row2Y, Size, Gap);
        DrawGroup(context, "Neutral", ds.Colors.Neutral, x + GroupStride * 2, row2Y, Size, Gap);

        // Row 3: Warning, Error
        NFloat row3Y = row2Y + SwatchBlockH + RowGap;
        DrawGroup(context, "Warning", ds.Colors.Warning, x, row3Y, Size, Gap);
        DrawGroup(context, "Error", ds.Colors.Error, x + GroupStride, row3Y, Size, Gap);
    }

    private static void DrawGroup(IContext context, string label, ColorGroup group,
        NFloat x, NFloat y, NFloat size, NFloat gap)
    {
        context.SetFont(new() { FontFamily = ["Inter"], FontSize = 11 });
        context.TextBaseline = TextBaseline.Top;
        context.SetFill(new Color(0x666666FF));
        context.FillText(label, new Point(x, y));

        y += 18;

        context.SetFill(group.Background);
        context.FillRect(new Rect(x, y, size, size));

        context.SetFill(group.Foreground);
        context.FillRect(new Rect(x + size + gap, y, size, size));

        context.SetFill(group.Container);
        context.FillRect(new Rect(x, y + size + gap, size, size));

        context.SetFill(group.OnContainer);
        context.FillRect(new Rect(x + size + gap, y + size + gap, size, size));
    }
}

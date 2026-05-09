using System.Runtime.InteropServices;

using Xui.Core.Canvas;
using Xui.Core.DI;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.Core.UI.Input;
using Xui.DevKit.UI.Design;

namespace Xui.Apps.TestApp.Examples.DesignSystem;

/// <summary>
/// Horizontal row of NeutralStyle options showing App/Surface color relationship.
/// </summary>
internal class NeutralStylePicker : View
{
    private int hoveredIndex = -1;

    private static readonly string[] styleNames =
        ["Mono", "2nd App", "2nd Surface", "3rd App", "3rd Surface"];
    private static readonly NeutralStyle[] styles =
        [NeutralStyle.Monochrome, NeutralStyle.SecondaryApp, NeutralStyle.SecondarySurface, NeutralStyle.TertiaryApp, NeutralStyle.TertiarySurface];

    private static readonly NFloat ItemWidth = 72;
    private static readonly NFloat ItemHeight = 48;
    private static readonly NFloat Gap = 6;

    public override int Count => 0;
    public override View this[int index] => throw new IndexOutOfRangeException();

    private int HitIndex(NFloat globalX)
    {
        var localX = globalX - this.Frame.X;
        var stride = ItemWidth + Gap;
        var idx = (int)(localX / stride);
        if (localX - idx * stride > ItemWidth) return -1;
        return idx >= 0 && idx < styles.Length ? idx : -1;
    }

    public override void OnPointerEvent(ref PointerEventRef e, EventPhase phase)
    {
        if (phase == EventPhase.Tunnel)
        {
            if (e.Type == PointerEventType.Down)
            {
                var editor = this.GetService<IDesignSystemEditor>();
                var idx = HitIndex(e.State.Position.X);
                if (idx >= 0 && editor != null)
                    editor.SetNeutralStyle(styles[idx]);
            }
            else if (e.Type == PointerEventType.Move || e.Type == PointerEventType.Enter)
            {
                var idx = HitIndex(e.State.Position.X);
                if (idx != hoveredIndex)
                {
                    hoveredIndex = idx;
                    this.InvalidateRender();
                }
            }
            else if (e.Type == PointerEventType.Leave)
            {
                if (hoveredIndex != -1)
                {
                    hoveredIndex = -1;
                    this.InvalidateRender();
                }
            }
        }
        base.OnPointerEvent(ref e, phase);
    }

    protected override Size MeasureCore(Size available, IMeasureContext context)
    {
        var totalWidth = styles.Length * ItemWidth + (styles.Length - 1) * Gap;
        return new Size(totalWidth, ItemHeight + 18);
    }

    protected override void RenderCore(IContext context)
    {
        var ds = this.GetService<IDesignSystem>();
        var editor = this.GetService<IDesignSystemEditor>();
        if (ds == null || editor == null) return;

        for (int i = 0; i < styles.Length; i++)
        {
            bool isActive = styles[i] == editor.NeutralStyle;
            bool isHovered = i == hoveredIndex;
            var x = this.Frame.X + i * (ItemWidth + Gap);
            var y = this.Frame.Y;

            var itemRect = new Rect(x, y, ItemWidth, ItemHeight);

            if (isActive)
            {
                context.BeginPath();
                context.RoundRect(itemRect, 6);
                context.SetFill(ds.Colors.Primary.Container);
                context.Fill(FillRule.NonZero);
                context.SetStroke(ds.Colors.Primary.Background);
                context.LineWidth = 1;
                context.Stroke();
            }
            else if (isHovered)
            {
                context.BeginPath();
                context.RoundRect(itemRect, 6);
                context.SetFill(ds.Colors.Surface.Container);
                context.Fill(FillRule.NonZero);
                context.SetStroke(ds.Colors.Outline);
                context.LineWidth = 1;
                context.Stroke();
            }

            // Mini preview: two stacked rects showing app (outer) and surface (inner)
            var outerRect = new Rect(x + 8, y + 6, ItemWidth - 16, 22);
            var innerRect = new Rect(x + 14, y + 12, ItemWidth - 28, 12);

            // Outer = app color, Inner = surface color (preview of what it would look like)
            var (appTint, surfaceTint) = GetPreviewTint(styles[i]);
            context.BeginPath();
            context.RoundRect(outerRect, 3);
            context.SetFill(appTint ? ds.Colors.Secondary.Container : new Color(0xF0F0F0FF));
            context.Fill(FillRule.NonZero);
            context.SetStroke(ds.Colors.Outline);
            context.LineWidth = 1;
            context.Stroke();

            context.BeginPath();
            context.RoundRect(innerRect, 2);
            context.SetFill(surfaceTint ? ds.Colors.Secondary.Container : new Color(0xFFFFFFFF));
            context.Fill(FillRule.NonZero);
            context.SetStroke(ds.Colors.OutlineVariant);
            context.LineWidth = 1;
            context.Stroke();

            // Label
            context.SetFont(new() { FontFamily = ["Inter"], FontSize = 9 });
            context.TextBaseline = TextBaseline.Top;
            context.SetFill(isActive ? ds.Colors.Primary.OnContainer : ds.Colors.Surface.Foreground);
            context.FillText(styleNames[i], new Point(x + 4, y + ItemHeight + 2));
        }
    }

    private static (bool appTint, bool surfaceTint) GetPreviewTint(NeutralStyle style) => style switch
    {
        NeutralStyle.Monochrome       => (false, false),
        NeutralStyle.SecondaryApp     => (true,  false),
        NeutralStyle.SecondarySurface => (false, true),
        NeutralStyle.TertiaryApp      => (true,  false),
        NeutralStyle.TertiarySurface  => (false, true),
        _                             => (false, false),
    };
}

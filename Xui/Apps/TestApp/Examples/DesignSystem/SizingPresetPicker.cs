using System.Runtime.InteropServices;

using Xui.Core.Canvas;
using Xui.Core.DI;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.Core.UI.Input;
using Xui.DevKit.UI.Design;

namespace Xui.Apps.TestApp.Examples.DesignSystem;

/// <summary>
/// Horizontal row of sizing preset options.
/// </summary>
internal class SizingPresetPicker : View
{
    private int hoveredIndex = -1;

    private static readonly string[] presetNames = ["Desktop", "Desktop+Touch", "Mobile"];
    private static readonly SizingPreset[] presets =
        [SizingPreset.Desktop, SizingPreset.TouchEnabled, SizingPreset.Mobile];

    private static readonly NFloat ItemWidth = 100;
    private static readonly NFloat ItemHeight = 48;
    private static readonly NFloat Gap = 8;

    public override int Count => 0;
    public override View this[int index] => throw new IndexOutOfRangeException();

    private int HitIndex(NFloat globalX)
    {
        var localX = globalX - this.Frame.X;
        var stride = ItemWidth + Gap;
        var idx = (int)(localX / stride);
        if (localX - idx * stride > ItemWidth) return -1;
        return idx >= 0 && idx < presets.Length ? idx : -1;
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
                    editor.SetSizingPreset(presets[idx]);
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
        var totalWidth = presets.Length * ItemWidth + (presets.Length - 1) * Gap;
        return new Size(totalWidth, ItemHeight + 18);
    }

    protected override void RenderCore(IContext context)
    {
        var ds = this.GetService<IDesignSystem>();
        var editor = this.GetService<IDesignSystemEditor>();
        if (ds == null || editor == null) return;

        for (int i = 0; i < presets.Length; i++)
        {
            bool isActive = presets[i] == editor.SizingPreset;
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

            // Mini preview: lines showing density
            var previewX = x + 8;
            var previewY = y + 6;
            var lineGap = GetPreviewLineGap(presets[i]);
            context.SetStroke(ds.Colors.Outline);
            context.LineWidth = 2;
            for (int j = 0; j < 4; j++)
            {
                var ly = previewY + j * lineGap;
                context.BeginPath();
                context.MoveTo(new Point(previewX, ly));
                context.LineTo(new Point(previewX + ItemWidth - 16, ly));
                context.Stroke();
            }

            // Hit target circle
            var hitR = GetPreviewHitRadius(presets[i]);
            if (hitR > 0)
            {
                var circleX = previewX + ItemWidth - 24;
                var circleY = previewY + 2 * lineGap;
                context.BeginPath();
                context.Arc(new Point(circleX, circleY), hitR, 0, (NFloat)(2 * Math.PI));
                context.SetStroke(ds.Colors.Primary.Background);
                context.LineWidth = 1;
                context.Stroke();
            }

            // Label
            context.SetFont(new() { FontFamily = ["Inter"], FontSize = 10 });
            context.TextBaseline = TextBaseline.Top;
            context.SetFill(isActive ? ds.Colors.Primary.OnContainer : ds.Colors.Surface.Foreground);
            context.FillText(presetNames[i], new Point(x + 4, y + ItemHeight + 2));
        }
    }

    private static NFloat GetPreviewLineGap(SizingPreset preset) => preset switch
    {
        SizingPreset.Desktop => 6,
        SizingPreset.TouchEnabled => 6,
        SizingPreset.Mobile => 10,
        _ => 8,
    };

    private static NFloat GetPreviewHitRadius(SizingPreset preset) => preset switch
    {
        SizingPreset.Desktop => 3,
        SizingPreset.TouchEnabled => 8,
        SizingPreset.Mobile => 8,
        _ => 4,
    };
}

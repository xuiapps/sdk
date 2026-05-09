using System.Runtime.InteropServices;

using Xui.Core.Canvas;
using Xui.Core.DI;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.Core.UI.Input;
using Xui.DevKit.UI.Design;

namespace Xui.Apps.TestApp.Examples.DesignSystem;

/// <summary>
/// Horizontal row of motion preset options.
/// </summary>
internal class MotionPresetPicker : View
{
    private int hoveredIndex = -1;

    private static readonly string[] presetNames = ["None", "Short", "Normal", "Long"];
    private static readonly MotionPreset[] presets =
        [MotionPreset.None, MotionPreset.Short, MotionPreset.Normal, MotionPreset.Long];

    private static readonly NFloat ItemWidth = 80;
    private static readonly NFloat ItemHeight = 40;
    private static readonly NFloat Gap = 6;

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
                    editor.SetMotionPreset(presets[idx]);
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
            bool isActive = presets[i] == editor.MotionPreset;
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

            // Mini speed indicator
            var barY = y + 10;
            var barWidth = GetPreviewBarWidth(presets[i]);
            context.BeginPath();
            context.RoundRect(new Rect(x + 10, barY, barWidth, 6), 3);
            context.SetFill(ds.Colors.Primary.Background);
            context.Fill(FillRule.NonZero);

            // "x" or bar for None
            if (presets[i] == MotionPreset.None)
            {
                context.BeginPath();
                context.MoveTo(new Point(x + 12, barY - 2));
                context.LineTo(new Point(x + 22, barY + 8));
                context.MoveTo(new Point(x + 22, barY - 2));
                context.LineTo(new Point(x + 12, barY + 8));
                context.SetStroke(ds.Colors.Error.Background);
                context.LineWidth = 1.5f;
                context.Stroke();
            }

            // Label
            context.SetFont(new() { FontFamily = ["Inter"], FontSize = 10 });
            context.TextBaseline = TextBaseline.Top;
            context.SetFill(isActive ? ds.Colors.Primary.OnContainer : ds.Colors.Surface.Foreground);
            context.FillText(presetNames[i], new Point(x + 4, y + ItemHeight + 2));
        }
    }

    private static NFloat GetPreviewBarWidth(MotionPreset preset) => preset switch
    {
        MotionPreset.None => 0,
        MotionPreset.Short => 20,
        MotionPreset.Normal => 40,
        MotionPreset.Long => 60,
        _ => 40,
    };
}

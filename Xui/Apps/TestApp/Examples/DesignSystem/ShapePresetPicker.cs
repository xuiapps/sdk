using System.Runtime.InteropServices;

using Xui.Core.Canvas;
using Xui.Core.DI;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.Core.UI.Input;
using Xui.DevKit.UI.Design;

namespace Xui.Apps.TestApp.Examples.DesignSystem;

/// <summary>
/// Horizontal row of shape preset options. Each shows a mini preview of the corner style.
/// </summary>
internal class ShapePresetPicker : View
{
    private int hoveredIndex = -1;

    private static readonly string[] presetNames =
        ["Square", "Desktop", "Rounded", "Round+Pill", "Soft"];
    private static readonly ShapePreset[] presets =
        [ShapePreset.Square, ShapePreset.Desktop, ShapePreset.Rounded, ShapePreset.RoundedPill, ShapePreset.Soft];

    private static readonly NFloat ItemWidth = 68;
    private static readonly NFloat ItemHeight = 56;
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
                    editor.SetShapePreset(presets[idx]);
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
            bool isActive = presets[i] == editor.ShapePreset;
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

            var previewRadius = GetPreviewRadius(presets[i]);
            var btnRadius = GetPreviewButtonRadius(presets[i]);

            var cardRect = new Rect(x + 8, y + 6, ItemWidth - 16, 24);
            context.BeginPath();
            context.RoundRect(cardRect, previewRadius);
            context.SetStroke(ds.Colors.Outline);
            context.LineWidth = 1;
            context.Stroke();

            var btnRect = new Rect(x + 12, y + 27, 20, 8);
            context.BeginPath();
            context.RoundRect(btnRect, btnRadius);
            context.SetFill(ds.Colors.Primary.Background);
            context.Fill(FillRule.NonZero);

            context.SetFont(new() { FontFamily = ["Inter"], FontSize = 10 });
            context.TextBaseline = TextBaseline.Top;
            context.SetFill(isActive ? ds.Colors.Primary.OnContainer : ds.Colors.Surface.Foreground);
            context.FillText(presetNames[i], new Point(x + 4, y + ItemHeight + 2));
        }
    }

    private static NFloat GetPreviewRadius(ShapePreset preset) => preset switch
    {
        ShapePreset.Square => 0,
        ShapePreset.Desktop => 4,
        ShapePreset.Rounded => 8,
        ShapePreset.RoundedPill => 8,
        ShapePreset.Soft => 12,
        _ => 0,
    };

    private static NFloat GetPreviewButtonRadius(ShapePreset preset) => preset switch
    {
        ShapePreset.Square => 0,
        ShapePreset.Desktop => 0,
        ShapePreset.Rounded => 2,
        ShapePreset.RoundedPill => 9999,
        ShapePreset.Soft => 9999,
        _ => 0,
    };
}

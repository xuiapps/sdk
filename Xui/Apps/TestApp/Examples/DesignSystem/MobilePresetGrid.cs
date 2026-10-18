using Xui.Core.Abstract.Events;
using Xui.Core.Canvas;
using Xui.Core.DI;
using Xui.Core.Math2D;
using Xui.Core.UI;
using Xui.Core.UI.Input;
using Xui.DevKit.UI.Design;
using NFloat = System.Runtime.InteropServices.NFloat;

namespace Xui.Apps.TestApp.Examples.DesignSystem;

/// <summary>Centered, uniform preset cells for the constrained mobile palette.</summary>
internal enum MobilePresetVisual { SurfaceStyle, Shape, Sizing, Motion }

internal sealed class MobilePresetGrid(
    MobilePresetVisual visualKind,
    (string Name, Func<IDesignSystemEditor, bool> IsSelected, Action<IDesignSystemEditor> Select)[] items) : View
{
    private const int Columns = 5;
    private static readonly NFloat Gap = 8;
    private static readonly NFloat MaximumCellWidth = 104;
    private static readonly NFloat CellHeight = 58;
    private int hoveredIndex = -1;

    public override int Count => 0;
    public override View this[int index] => throw new IndexOutOfRangeException();

    protected override Size MeasureCore(Size available, IMeasureContext context) =>
        new(available.Width, GetRows() * (CellHeight + Gap) - Gap);

    public override void OnPointerEvent(ref PointerEventRef e, EventPhase phase)
    {
        var index = HitIndex(e.State.Position);
        if (e.Type is PointerEventType.Move or PointerEventType.Enter)
        {
            if (hoveredIndex != index) { hoveredIndex = index; InvalidateRender(); }
        }
        else if (e.Type == PointerEventType.Leave && hoveredIndex != -1)
        {
            hoveredIndex = -1;
            InvalidateRender();
        }
        else if (phase == EventPhase.Tunnel && e.Type == PointerEventType.Down && index >= 0)
        {
            var editor = ((IServiceProvider)this).GetService<IDesignSystemEditor>();
            if (editor is not null)
                items[index].Select(editor);
        }
    }

    protected override void RenderCore(IContext context)
    {
        var design = ((IServiceProvider)this).GetService<IDesignSystem>();
        var editor = ((IServiceProvider)this).GetService<IDesignSystemEditor>();
        if (design is null || editor is null) return;

        for (var index = 0; index < items.Length; index++)
        {
            var frame = GetFrame(index);
            var selected = items[index].IsSelected(editor);
            context.BeginPath();
            context.RoundRect(frame, 10);
            context.SetFill(selected ? design.Colors.Primary.Container : hoveredIndex == index ? design.Colors.Surface.Container : design.Colors.Surface.Background);
            context.Fill();
            context.BeginPath();
            context.RoundRect(frame, 10);
            context.SetStroke(selected ? design.Colors.Primary.Background : design.Colors.OutlineVariant);
            context.LineWidth = 1;
            context.Stroke();
            DrawVisual(context, frame, index, design);
            context.SetFont(new Font(9, "Inter", FontWeight.Medium));
            context.TextAlign = TextAlign.Center;
            context.TextBaseline = TextBaseline.Middle;
            context.SetFill(selected ? design.Colors.Primary.OnContainer : design.Colors.Surface.Foreground);
            context.FillText(items[index].Name, new Point(frame.Center.X, frame.Bottom - 9));
        }
    }

    private int GetRows() => (items.Length + Columns - 1) / Columns;

    private Rect GetFrame(int index)
    {
        var row = index / Columns;
        var first = row * Columns;
        var count = NFloat.Min(Columns, items.Length - first);
        var width = NFloat.Min(MaximumCellWidth, (Frame.Width - (count - 1) * Gap) / count);
        var contentWidth = count * width + (count - 1) * Gap;
        var column = index - first;
        return new Rect(Frame.X + (Frame.Width - contentWidth) / 2 + column * (width + Gap), Frame.Y + row * (CellHeight + Gap), width, CellHeight);
    }

    private int HitIndex(Point point)
    {
        for (var index = 0; index < items.Length; index++)
            if (GetFrame(index).Contains(point)) return index;
        return -1;
    }

    private void DrawVisual(IContext context, Rect frame, int index, IDesignSystem design)
    {
        var preview = new Rect(frame.X + 8, frame.Y + 7, frame.Width - 16, 27);
        switch (visualKind)
        {
            case MobilePresetVisual.Shape:
                var radius = index switch { 0 => 0, 1 => 3, 2 => 7, 3 => 999, _ => 12 };
                context.BeginPath();
                context.RoundRect(preview, radius);
                context.SetStroke(design.Colors.Outline);
                context.LineWidth = 1.5f;
                context.Stroke();
                context.BeginPath();
                context.RoundRect(new Rect(preview.X + 6, preview.Y + 10, preview.Width - 12, 7), index >= 3 ? 999 : radius / 2);
                context.SetFill(design.Colors.Primary.Background);
                context.Fill();
                break;
            case MobilePresetVisual.Sizing:
                var gap = index switch { 0 => 5, 1 => 5, _ => 8 };
                for (var line = 0; line < 3; line++)
                {
                    var y = preview.Y + 4 + line * gap;
                    context.BeginPath();
                    context.MoveTo((preview.X + 3, y));
                    context.LineTo((preview.Right - 3, y));
                    context.SetStroke(design.Colors.Outline);
                    context.LineWidth = index == 0 ? 1 : 2;
                    context.Stroke();
                }
                if (index > 0)
                {
                    context.BeginPath();
                    context.Arc((preview.Right - 7, preview.Center.Y), index == 1 ? 4 : 6, 0, NFloat.Pi * 2);
                    context.SetStroke(design.Colors.Primary.Background);
                    context.LineWidth = 1;
                    context.Stroke();
                }
                break;
            case MobilePresetVisual.Motion:
                if (index == 0)
                {
                    context.BeginPath();
                    context.MoveTo((preview.X + 5, preview.Y + 5));
                    context.LineTo((preview.Right - 5, preview.Bottom - 5));
                    context.MoveTo((preview.Right - 5, preview.Y + 5));
                    context.LineTo((preview.X + 5, preview.Bottom - 5));
                    context.SetStroke(design.Colors.Error.Background);
                    context.LineWidth = 1.5f;
                    context.Stroke();
                    break;
                }
                var trail = index switch { 1 => 1, 2 => 2, _ => 3 };
                for (var dot = 0; dot < trail; dot++)
                {
                    var trailRadius = 2 + dot;
                    context.BeginPath();
                    context.Arc((preview.X + 7 + dot * 7, preview.Center.Y), trailRadius, 0, NFloat.Pi * 2);
                    context.SetFill(design.Colors.Primary.Container);
                    context.Fill();
                }
                context.BeginPath();
                context.Arc((preview.Right - 7, preview.Center.Y), 5, 0, NFloat.Pi * 2);
                context.SetFill(design.Colors.Primary.Background);
                context.Fill();
                break;
            default:
                var app = index is 1 or 2 ? design.Colors.Secondary.Container : index is 3 or 4 ? design.Colors.Tertiary.Container : design.Colors.Application.Background;
                var surface = index is 2 or 4 ? index == 2 ? design.Colors.Secondary.Container : design.Colors.Tertiary.Container : design.Colors.Surface.Background;
                context.BeginPath();
                context.RoundRect(preview, 5);
                context.SetFill(app);
                context.Fill();
                context.BeginPath();
                context.RoundRect(preview.Expand(-5), 3);
                context.SetFill(surface);
                context.Fill();
                break;
        }
    }
}
